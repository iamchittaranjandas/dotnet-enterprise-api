using System.Collections.Concurrent;
using System.Diagnostics;
using DotnetEnterpriseApi.Application.Features.Tasks.AgentTools;
using DotnetEnterpriseApi.Application.Features.Workflow.Definitions;
using DotnetEnterpriseApi.Application.Features.Workflow.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetEnterpriseApi.Infrastructure.Workflow
{
    /// <summary>
    /// Executes workflow definitions step-by-step against the active LLM provider.
    ///
    /// Step execution:
    ///   LlmStep      — single chat completion, output appended to AccumulatedContext
    ///   ToolStep     — Microsoft Agent with the named tool set; output appended to context
    ///   ConditionStep — single LLM call whose trimmed reply selects the next step branch
    ///
    /// Executions are kept in-memory (ConcurrentDictionary) and can be polled by ID.
    /// For production: replace the in-memory store with a database-backed execution log.
    /// </summary>
    public class WorkflowEngine : IWorkflowEngine
    {
        private readonly IChatClient _chatClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<WorkflowEngine> _logger;

        private readonly IReadOnlyDictionary<string, WorkflowDefinition> _definitions;
        private readonly ConcurrentDictionary<string, WorkflowExecution> _executions = new();

        public WorkflowEngine(
            IChatClient chatClient,
            IServiceScopeFactory scopeFactory,
            ILoggerFactory loggerFactory)
        {
            _chatClient   = chatClient;
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
            _logger        = loggerFactory.CreateLogger<WorkflowEngine>();

            _definitions = BuiltInWorkflows.All.ToDictionary(w => w.Name, w => w);
        }

        // ── Public API ────────────────────────────────────────────────────────

        public IReadOnlyList<WorkflowDefinition> GetDefinitions() =>
            _definitions.Values.ToList();

        public WorkflowExecution? GetExecution(string executionId) =>
            _executions.GetValueOrDefault(executionId);

        public async Task<WorkflowExecution> RunAsync(
            string workflowName,
            string input,
            CancellationToken cancellationToken = default)
        {
            if (!_definitions.TryGetValue(workflowName, out var definition))
                throw new InvalidOperationException($"Workflow '{workflowName}' not found.");

            var execution = new WorkflowExecution
            {
                WorkflowName = workflowName,
                Input        = input,
            };
            _executions[execution.ExecutionId] = execution;

            _logger.LogInformation(
                "Workflow '{Name}' started. ExecutionId={Id}, Input='{Input}'",
                workflowName, execution.ExecutionId, input.Length > 80 ? input[..80] + "…" : input);

            try
            {
                await ExecuteWorkflowAsync(definition, execution, cancellationToken);

                execution.Status      = WorkflowStatus.Completed;
                execution.FinalOutput = execution.AccumulatedContext;
                execution.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Workflow '{Name}' completed. ExecutionId={Id}, Steps={Count}",
                    workflowName, execution.ExecutionId, execution.StepHistory.Count);
            }
            catch (Exception ex)
            {
                execution.Status      = WorkflowStatus.Failed;
                execution.Error       = ex.Message;
                execution.CompletedAt = DateTime.UtcNow;

                _logger.LogError(ex,
                    "Workflow '{Name}' failed at step. ExecutionId={Id}",
                    workflowName, execution.ExecutionId);
            }

            return execution;
        }

        // ── Execution loop ────────────────────────────────────────────────────

        private async Task ExecuteWorkflowAsync(
            WorkflowDefinition definition,
            WorkflowExecution execution,
            CancellationToken cancellationToken)
        {
            var stepMap     = definition.Steps.ToDictionary(s => s.Name);
            var currentName = definition.EntryStep;

            // Guard against infinite loops in mis-configured workflows
            const int maxSteps = 20;
            var stepCount = 0;

            while (currentName is not null && stepCount < maxSteps)
            {
                if (!stepMap.TryGetValue(currentName, out var step))
                    throw new InvalidOperationException($"Step '{currentName}' not found in workflow '{definition.Name}'.");

                cancellationToken.ThrowIfCancellationRequested();

                var prompt = BuildPrompt(step.PromptTemplate, execution);

                _logger.LogDebug(
                    "Workflow '{Name}' executing step '{Step}' ({Type})",
                    definition.Name, step.Name, step.Type);

                var sw = Stopwatch.StartNew();

                var (output, branch) = step.Type switch
                {
                    WorkflowStepType.Llm       => await RunLlmStepAsync(prompt, cancellationToken),
                    WorkflowStepType.Tool       => await RunToolStepAsync(step, prompt, cancellationToken),
                    WorkflowStepType.Condition  => await RunConditionStepAsync(step, prompt, cancellationToken),
                    _                           => throw new InvalidOperationException($"Unknown step type: {step.Type}"),
                };

                sw.Stop();

                execution.StepHistory.Add(new WorkflowStepResult
                {
                    StepName    = step.Name,
                    StepType    = step.Type,
                    Output      = output,
                    BranchTaken = branch,
                    Duration    = sw.Elapsed,
                });

                // Append output to accumulated context for subsequent steps
                execution.AccumulatedContext +=
                    $"\n\n[Step: {step.Name}]\n{output}";

                _logger.LogInformation(
                    "Workflow step '{Step}' done in {Ms}ms. Branch={Branch}",
                    step.Name, sw.ElapsedMilliseconds, branch ?? "—");

                // Determine next step
                currentName = branch is not null && step.Branches.TryGetValue(branch, out var branchTarget)
                    ? branchTarget
                    : step.NextStep;

                stepCount++;
            }
        }

        // ── Step runners ──────────────────────────────────────────────────────

        private async Task<(string Output, string? Branch)> RunLlmStepAsync(
            string prompt,
            CancellationToken cancellationToken)
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System,
                    "You are an AI workflow automation assistant. Follow the instructions precisely."),
                new(ChatRole.User, prompt),
            };

            var response = await _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
            return (response.Text.Trim(), null);
        }

        private async Task<(string Output, string? Branch)> RunConditionStepAsync(
            WorkflowStepDefinition step,
            string prompt,
            CancellationToken cancellationToken)
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System,
                    "You are a classifier. Reply with a single word only — no explanation."),
                new(ChatRole.User, prompt),
            };

            var response = await _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
            var branch   = response.Text.Trim().ToLowerInvariant();

            // Normalise to a known branch key
            var matched = step.Branches.Keys
                .FirstOrDefault(k => branch.Contains(k, StringComparison.OrdinalIgnoreCase))
                ?? branch;

            return (branch, matched);
        }

        private async Task<(string Output, string? Branch)> RunToolStepAsync(
            WorkflowStepDefinition step,
            string prompt,
            CancellationToken cancellationToken)
        {
            // Resolve tool set inside a fresh scope (tools are scoped services)
            await using var scope = _scopeFactory.CreateAsyncScope();
            var tools = ResolveTools(step.ToolSet, scope.ServiceProvider);

            var agent    = _chatClient.AsAIAgent(
                name: $"WorkflowAgent-{step.Name}",
                instructions:
                    "You are an AI workflow automation agent. " +
                    "Use the available tools to complete the task. " +
                    "Be precise and confirm every action with its result.",
                description: $"Workflow step: {step.Name}",
                tools: tools,
                loggerFactory: _loggerFactory);

            var session  = await agent.CreateSessionAsync(cancellationToken);
            var response = await agent.RunAsync(prompt, session, cancellationToken: cancellationToken);

            return (response.ToString().Trim(), null);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string BuildPrompt(string template, WorkflowExecution execution) =>
            template
                .Replace("{input}",   execution.Input)
                .Replace("{context}", execution.AccumulatedContext.TrimStart());

        private static List<AITool> ResolveTools(string? toolSet, IServiceProvider sp)
        {
            return toolSet?.ToLowerInvariant() switch
            {
                "query" => BuildTools(sp.GetRequiredService<QueryAgentTools>(), "query"),
                "mutation" => BuildTools(sp.GetRequiredService<MutationAgentTools>(), "mutation"),
                "all" => [
                    ..BuildTools(sp.GetRequiredService<QueryAgentTools>(), "query"),
                    ..BuildTools(sp.GetRequiredService<MutationAgentTools>(), "mutation"),
                ],
                _ => [],
            };
        }

        private static List<AITool> BuildTools(object tools, string set) => set switch
        {
            "query" when tools is QueryAgentTools q =>
            [
                AIFunctionFactory.Create(q.ListTasksAsync),
                AIFunctionFactory.Create(q.GetTaskByIdAsync),
                AIFunctionFactory.Create(q.CountTasksAsync),
            ],
            "mutation" when tools is MutationAgentTools m =>
            [
                AIFunctionFactory.Create(m.CreateTaskAsync),
                AIFunctionFactory.Create(m.UpdateTaskCompletionAsync),
                AIFunctionFactory.Create(m.UpdateTaskDetailsAsync),
                AIFunctionFactory.Create(m.DeleteTaskAsync),
            ],
            _ => [],
        };
    }
}
