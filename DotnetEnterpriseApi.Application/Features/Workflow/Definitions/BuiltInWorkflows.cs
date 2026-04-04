using DotnetEnterpriseApi.Application.Features.Workflow.Models;

namespace DotnetEnterpriseApi.Application.Features.Workflow.Definitions
{
    /// <summary>
    /// Registers the three built-in task automation workflows.
    ///
    /// triage        — Analyse a raw request, classify priority, and auto-create tasks
    /// daily-briefing — Summarise all tasks and produce an actionable daily plan
    /// auto-close    — Identify stale/redundant tasks and mark them complete
    /// </summary>
    public static class BuiltInWorkflows
    {
        public static IReadOnlyList<WorkflowDefinition> All { get; } =
        [
            Triage,
            DailyBriefing,
            AutoClose,
        ];

        // ── Triage ────────────────────────────────────────────────────────────

        private static WorkflowDefinition Triage { get; } = new()
        {
            Name        = "triage",
            Description = "Analyse a raw feature request or bug report, classify its priority, and auto-create one or more tasks.",
            EntryStep   = "classify",
            Steps =
            [
                new()
                {
                    Name = "classify",
                    Type = WorkflowStepType.Condition,
                    PromptTemplate =
                        """
                        You are a senior engineering triage bot.
                        Analyse the following request and classify it into exactly one of: critical, high, medium, low.

                        Request: {input}

                        Reply with ONLY the single word: critical, high, medium, or low.
                        """,
                    Branches = new()
                    {
                        ["critical"] = "create-critical",
                        ["high"]     = "create-high",
                        ["medium"]   = "create-medium",
                        ["low"]      = "create-low",
                    },
                    NextStep = "create-medium",
                },
                new()
                {
                    Name = "create-critical",
                    Type = WorkflowStepType.Tool,
                    ToolSet = "mutation",
                    PromptTemplate =
                        """
                        PRIORITY: CRITICAL 🔴
                        The following request has been classified as critical.
                        Create a task with a clear title and detailed description.
                        Prefix the title with "[CRITICAL]".

                        Request: {input}
                        """,
                    NextStep = "summarise",
                },
                new()
                {
                    Name = "create-high",
                    Type = WorkflowStepType.Tool,
                    ToolSet = "mutation",
                    PromptTemplate =
                        """
                        PRIORITY: HIGH 🟠
                        The following request has been classified as high priority.
                        Create a task with a clear title and detailed description.
                        Prefix the title with "[HIGH]".

                        Request: {input}
                        """,
                    NextStep = "summarise",
                },
                new()
                {
                    Name = "create-medium",
                    Type = WorkflowStepType.Tool,
                    ToolSet = "mutation",
                    PromptTemplate =
                        """
                        PRIORITY: MEDIUM 🟡
                        The following request has been classified as medium priority.
                        Create a task with a clear title and detailed description.

                        Request: {input}
                        """,
                    NextStep = "summarise",
                },
                new()
                {
                    Name = "create-low",
                    Type = WorkflowStepType.Tool,
                    ToolSet = "mutation",
                    PromptTemplate =
                        """
                        PRIORITY: LOW 🟢
                        The following request has been classified as low priority.
                        Create a task with a clear title and description.
                        Prefix the title with "[LOW]".

                        Request: {input}
                        """,
                    NextStep = "summarise",
                },
                new()
                {
                    Name = "summarise",
                    Type = WorkflowStepType.Llm,
                    PromptTemplate =
                        """
                        You just triaged the following request:
                        {input}

                        Steps completed:
                        {context}

                        Write a brief (2-3 sentence) triage report: what was created, its priority, and what action is expected next.
                        """,
                    NextStep = null,
                },
            ],
        };

        // ── Daily Briefing ────────────────────────────────────────────────────

        private static WorkflowDefinition DailyBriefing { get; } = new()
        {
            Name        = "daily-briefing",
            Description = "Fetch all tasks, produce a prioritised daily plan, and identify blockers.",
            EntryStep   = "fetch",
            Steps =
            [
                new()
                {
                    Name = "fetch",
                    Type = WorkflowStepType.Tool,
                    ToolSet = "query",
                    PromptTemplate =
                        """
                        Fetch all tasks using the ListTasks tool (pageSize=50).
                        Return the raw list — do not summarise yet.
                        """,
                    NextStep = "prioritise",
                },
                new()
                {
                    Name = "prioritise",
                    Type = WorkflowStepType.Llm,
                    PromptTemplate =
                        """
                        You are a productivity coach.
                        Below is the full task list:
                        {context}

                        Produce a concise daily plan:
                        1. TOP 3 tasks to focus on today (with a one-line reason each)
                        2. Tasks that are overdue or likely blocking others
                        3. Tasks that can be deferred or are already done

                        Be direct and actionable. Use bullet points.
                        """,
                    NextStep = "identify-blockers",
                },
                new()
                {
                    Name = "identify-blockers",
                    Type = WorkflowStepType.Llm,
                    PromptTemplate =
                        """
                        Based on the task list and the daily plan below, identify any potential blockers
                        or dependencies between tasks. Suggest one concrete next action to unblock progress.

                        Context so far:
                        {context}
                        """,
                    NextStep = null,
                },
            ],
        };

        // ── Auto-Close ────────────────────────────────────────────────────────

        private static WorkflowDefinition AutoClose { get; } = new()
        {
            Name        = "auto-close",
            Description = "Identify tasks that appear complete or stale based on their description, and mark them as completed.",
            EntryStep   = "fetch",
            Steps =
            [
                new()
                {
                    Name = "fetch",
                    Type = WorkflowStepType.Tool,
                    ToolSet = "query",
                    PromptTemplate =
                        """
                        Use the ListTasks tool to fetch all pending tasks (pageSize=50).
                        Return the full list.
                        """,
                    NextStep = "analyse",
                },
                new()
                {
                    Name = "analyse",
                    Type = WorkflowStepType.Llm,
                    PromptTemplate =
                        """
                        You are a task hygiene bot.
                        Review the following pending tasks:
                        {context}

                        Identify tasks that:
                        - Are described in past tense (suggesting they are already done)
                        - Have placeholder or test-like titles (e.g. "test", "temp", "TODO")
                        - Are duplicates of other tasks

                        List the IDs of tasks that should be marked as completed, one per line, in the format:
                        CLOSE: <id>

                        If no tasks should be closed, reply with: NONE
                        """,
                    NextStep = "close",
                },
                new()
                {
                    Name = "close",
                    Type = WorkflowStepType.Tool,
                    ToolSet = "mutation",
                    PromptTemplate =
                        """
                        The analysis identified the following tasks to close:
                        {context}

                        For each line starting with "CLOSE: <id>", call UpdateTaskCompletion to mark that task as completed.
                        If the analysis said NONE, do nothing and reply "No tasks to close."
                        """,
                    NextStep = "report",
                },
                new()
                {
                    Name = "report",
                    Type = WorkflowStepType.Llm,
                    PromptTemplate =
                        """
                        Write a brief auto-close report based on what happened:
                        {context}

                        Include: how many tasks were reviewed, how many were closed, and why.
                        """,
                    NextStep = null,
                },
            ],
        };
    }
}
