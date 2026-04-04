using Asp.Versioning;
using DotnetEnterpriseApi.Application.Features.Tasks.AgentTools;
using DotnetEnterpriseApi.Application.Features.Tasks.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace DotnetEnterpriseApi.Api.Controllers
{
    /// <summary>
    /// Multi-agent endpoint (v2).
    ///
    /// Architecture:
    ///   POST /api/v2/multiagent/chat
    ///     → OrchestratorAgent  (classifies intent: query | mutation | both)
    ///     → QueryAgent         (read: list, get, count — RAG-augmented)
    ///     → MutationAgent      (write: create, update, delete)
    ///     ← merged reply
    ///
    /// The Orchestrator reasons over the user's message to decide which sub-agent(s)
    /// to delegate to, then combines their responses into a single coherent reply.
    /// </summary>
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class MultiAgentController : ControllerBase
    {
        private readonly IChatClient _chatClient;
        private readonly QueryAgentTools _queryTools;
        private readonly MutationAgentTools _mutationTools;
        private readonly ITaskRagService _ragService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MultiAgentController> _logger;

        public MultiAgentController(
            IChatClient chatClient,
            QueryAgentTools queryTools,
            MutationAgentTools mutationTools,
            ITaskRagService ragService,
            ILoggerFactory loggerFactory)
        {
            _chatClient    = chatClient;
            _queryTools    = queryTools;
            _mutationTools = mutationTools;
            _ragService    = ragService;
            _loggerFactory = loggerFactory;
            _logger        = loggerFactory.CreateLogger<MultiAgentController>();
        }

        /// <summary>
        /// Send a natural-language message to the multi-agent system.
        /// The orchestrator automatically routes to the correct sub-agent(s).
        /// </summary>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat(
            [FromBody] MultiAgentChatRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Message cannot be empty." });

            // 1. RAG — retrieve semantic context for query augmentation
            var ragResult = await _ragService.RetrieveContextAsync(
                request.Message,
                topK: 5,
                filterCompleted: request.FilterCompleted,
                hybrid: true,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "MultiAgent: RAG returned {Count} context item(s) for '{Message}'",
                ragResult.Matches.Count, request.Message);

            // 2. Classify intent via the Orchestrator
            var intent = await ClassifyIntentAsync(request.Message, cancellationToken);
            _logger.LogInformation("MultiAgent: Classified intent as '{Intent}' for '{Message}'", intent, request.Message);

            // 3. Route to sub-agent(s)
            string reply;
            string agentUsed;

            switch (intent)
            {
                case AgentIntent.Query:
                    (reply, agentUsed) = await RunQueryAgentAsync(request.Message, ragResult, cancellationToken);
                    break;

                case AgentIntent.Mutation:
                    (reply, agentUsed) = await RunMutationAgentAsync(request.Message, cancellationToken);
                    break;

                case AgentIntent.Both:
                default:
                    // Mutation first (state change), then query for confirmation
                    var (mutReply, _)   = await RunMutationAgentAsync(request.Message, cancellationToken);
                    var (queryReply, _) = await RunQueryAgentAsync(request.Message, ragResult, cancellationToken);
                    reply     = $"{mutReply}\n\n{queryReply}";
                    agentUsed = "MutationAgent + QueryAgent";
                    break;
            }

            return Ok(new MultiAgentChatResponse
            {
                Reply      = reply,
                AgentUsed  = agentUsed,
                Intent     = intent.ToString(),
                RagContext  = ragResult.Matches.Select(m => new MultiAgentRagItem
                {
                    TaskId      = m.TaskId,
                    Title       = m.Title,
                    IsCompleted = m.IsCompleted,
                    Score       = MathF.Round(m.Score, 4),
                }).ToList(),
            });
        }

        // ── Intent classification ─────────────────────────────────────────────

        private async Task<AgentIntent> ClassifyIntentAsync(string message, CancellationToken cancellationToken)
        {
            var classifierInstructions =
                """
                You are an intent classifier for a task management system.
                Classify the user's message into exactly one of these three categories:

                  query    — user wants to READ, LIST, SEARCH, COUNT, or GET information about tasks
                  mutation — user wants to CREATE, UPDATE, COMPLETE, DELETE, or MODIFY tasks
                  both     — the message requires both reading AND writing (e.g. "create a task and show me all pending ones")

                Reply with ONLY the single word: query, mutation, or both.
                Do not include any other text.
                """;

            var classifier = _chatClient.AsAIAgent(
                name: "IntentClassifier",
                instructions: classifierInstructions,
                description: "Classifies task management intent",
                loggerFactory: _loggerFactory);

            var session  = await classifier.CreateSessionAsync(cancellationToken);
            var response = await classifier.RunAsync(message, session, cancellationToken: cancellationToken);
            var raw      = response.ToString().Trim().ToLowerInvariant();

            return raw switch
            {
                "mutation" => AgentIntent.Mutation,
                "both"     => AgentIntent.Both,
                _          => AgentIntent.Query,
            };
        }

        // ── QueryAgent ────────────────────────────────────────────────────────

        private async Task<(string Reply, string AgentName)> RunQueryAgentAsync(
            string message,
            RagRetrievalResult ragResult,
            CancellationToken cancellationToken)
        {
            var contextBlock = ragResult.HasContext
                ? $"\n\nSemantically relevant tasks (use as read context):\n{ragResult.Context}"
                : string.Empty;

            var instructions =
                $"""
                You are a read-only task query assistant.
                Your job is to answer questions about tasks, list them, search them, and summarise them.
                Never create, modify, or delete tasks — use the available tools only for reading.
                Be concise and use the context below when relevant.
                {contextBlock}
                """;

            var tools = new List<AITool>
            {
                AIFunctionFactory.Create(_queryTools.ListTasksAsync),
                AIFunctionFactory.Create(_queryTools.GetTaskByIdAsync),
                AIFunctionFactory.Create(_queryTools.CountTasksAsync),
            };

            var agent    = _chatClient.AsAIAgent("QueryAgent", instructions, "Reads tasks", tools, _loggerFactory);
            var session  = await agent.CreateSessionAsync(cancellationToken);
            var response = await agent.RunAsync(message, session, cancellationToken: cancellationToken);

            return (response.ToString(), "QueryAgent");
        }

        // ── MutationAgent ─────────────────────────────────────────────────────

        private async Task<(string Reply, string AgentName)> RunMutationAgentAsync(
            string message,
            CancellationToken cancellationToken)
        {
            const string instructions =
                """
                You are a task mutation assistant.
                Your job is to create, update, and delete tasks based on user instructions.
                Always confirm what you did with the result returned by each tool.
                Be concise and professional.
                """;

            var tools = new List<AITool>
            {
                AIFunctionFactory.Create(_mutationTools.CreateTaskAsync),
                AIFunctionFactory.Create(_mutationTools.UpdateTaskCompletionAsync),
                AIFunctionFactory.Create(_mutationTools.UpdateTaskDetailsAsync),
                AIFunctionFactory.Create(_mutationTools.DeleteTaskAsync),
            };

            var agent    = _chatClient.AsAIAgent("MutationAgent", instructions, "Writes tasks", tools, _loggerFactory);
            var session  = await agent.CreateSessionAsync(cancellationToken);
            var response = await agent.RunAsync(message, session, cancellationToken: cancellationToken);

            return (response.ToString(), "MutationAgent");
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    public class MultiAgentChatRequest
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>RAG filter: true=completed, false=pending, null=all.</summary>
        public bool? FilterCompleted { get; set; }
    }

    public class MultiAgentChatResponse
    {
        public string Reply { get; set; } = string.Empty;

        /// <summary>Which sub-agent(s) handled this request.</summary>
        public string AgentUsed { get; set; } = string.Empty;

        /// <summary>Classified intent: Query, Mutation, or Both.</summary>
        public string Intent { get; set; } = string.Empty;

        /// <summary>RAG context items injected into the QueryAgent's instructions.</summary>
        public IReadOnlyList<MultiAgentRagItem> RagContext { get; set; } = [];
    }

    public class MultiAgentRagItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public float Score { get; set; }
    }

    internal enum AgentIntent { Query, Mutation, Both }
}
