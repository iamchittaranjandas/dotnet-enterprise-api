namespace DotnetEnterpriseApi.Application.Features.Workflow.Models
{
    // ── Step types ────────────────────────────────────────────────────────────

    public enum WorkflowStepType
    {
        /// <summary>Pure LLM reasoning — no tools, just a prompt + response.</summary>
        Llm,

        /// <summary>Agent with tools — LLM can call registered tools.</summary>
        Tool,

        /// <summary>LLM classifies the current context and branches to a named next step.</summary>
        Condition,
    }

    // ── Definition (design-time, static) ─────────────────────────────────────

    public class WorkflowStepDefinition
    {
        public string Name { get; init; } = string.Empty;
        public WorkflowStepType Type { get; init; }

        /// <summary>
        /// Prompt template. Supports {input} (original workflow input) and
        /// {context} (accumulated output from all previous steps).
        /// </summary>
        public string PromptTemplate { get; init; } = string.Empty;

        /// <summary>For ConditionStep: map of LLM reply keyword → next step name.</summary>
        public Dictionary<string, string> Branches { get; init; } = new();

        /// <summary>Default next step name (null = end of workflow).</summary>
        public string? NextStep { get; init; }

        /// <summary>Tool set key for ToolStep (resolved by the engine).</summary>
        public string? ToolSet { get; init; }
    }

    public class WorkflowDefinition
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string EntryStep { get; init; } = string.Empty;
        public IReadOnlyList<WorkflowStepDefinition> Steps { get; init; } = [];
    }

    // ── Execution (runtime, per-run) ──────────────────────────────────────────

    public enum WorkflowStatus { Running, Completed, Failed }

    public class WorkflowStepResult
    {
        public string StepName { get; init; } = string.Empty;
        public WorkflowStepType StepType { get; init; }
        public string Output { get; init; } = string.Empty;
        public string? BranchTaken { get; init; }
        public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
        public TimeSpan Duration { get; init; }
    }

    public class WorkflowExecution
    {
        public string ExecutionId { get; init; } = Guid.NewGuid().ToString("N")[..12];
        public string WorkflowName { get; init; } = string.Empty;
        public string Input { get; init; } = string.Empty;
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Running;
        public string? FinalOutput { get; set; }
        public string? Error { get; set; }
        public List<WorkflowStepResult> StepHistory { get; init; } = [];
        public DateTime StartedAt { get; init; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        /// <summary>Accumulated context string fed into each subsequent step's prompt.</summary>
        public string AccumulatedContext { get; set; } = string.Empty;
    }
}
