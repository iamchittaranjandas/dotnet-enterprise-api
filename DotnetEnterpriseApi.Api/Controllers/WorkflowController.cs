using Asp.Versioning;
using DotnetEnterpriseApi.Application.Features.Workflow.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetEnterpriseApi.Api.Controllers
{
    /// <summary>
    /// AI Workflow Automation Engine (v2).
    ///
    /// POST /api/v2/workflow/run           — run a named workflow
    /// GET  /api/v2/workflow/{executionId} — poll execution status + step history
    /// GET  /api/v2/workflow/definitions   — list all registered workflows
    /// </summary>
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowEngine _engine;
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(IWorkflowEngine engine, ILogger<WorkflowController> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        /// <summary>
        /// Returns all registered workflow definitions.
        /// </summary>
        [HttpGet("definitions")]
        public IActionResult GetDefinitions()
        {
            var defs = _engine.GetDefinitions().Select(d => new
            {
                d.Name,
                d.Description,
                d.EntryStep,
                Steps = d.Steps.Select(s => new { s.Name, s.Type, s.NextStep }),
            });
            return Ok(defs);
        }

        /// <summary>
        /// Runs a workflow synchronously and returns the full execution result.
        /// For long-running workflows consider using a background task — this blocks until completion.
        /// </summary>
        [HttpPost("run")]
        public async Task<IActionResult> Run(
            [FromBody] WorkflowRunRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.WorkflowName))
                return BadRequest(new { message = "WorkflowName is required." });

            if (string.IsNullOrWhiteSpace(request.Input))
                return BadRequest(new { message = "Input is required." });

            _logger.LogInformation(
                "Workflow run requested: '{Name}' by {User}",
                request.WorkflowName, User.Identity?.Name ?? "anonymous");

            try
            {
                var execution = await _engine.RunAsync(
                    request.WorkflowName,
                    request.Input,
                    cancellationToken);

                return Ok(MapResponse(execution));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Returns the status and full step history of a previous execution.
        /// </summary>
        [HttpGet("{executionId}")]
        public IActionResult GetExecution(string executionId)
        {
            var execution = _engine.GetExecution(executionId);
            if (execution is null)
                return NotFound(new { message = $"Execution '{executionId}' not found." });

            return Ok(MapResponse(execution));
        }

        // ── Mapping ───────────────────────────────────────────────────────────

        private static WorkflowExecutionResponse MapResponse(WorkflowExecution e) => new()
        {
            ExecutionId  = e.ExecutionId,
            WorkflowName = e.WorkflowName,
            Status       = e.Status.ToString(),
            Input        = e.Input,
            FinalOutput  = e.FinalOutput,
            Error        = e.Error,
            StartedAt    = e.StartedAt,
            CompletedAt  = e.CompletedAt,
            DurationMs   = e.CompletedAt.HasValue
                ? (long)(e.CompletedAt.Value - e.StartedAt).TotalMilliseconds
                : null,
            Steps = e.StepHistory.Select(s => new WorkflowStepResponse
            {
                StepName    = s.StepName,
                StepType    = s.StepType.ToString(),
                Output      = s.Output,
                BranchTaken = s.BranchTaken,
                CompletedAt = s.CompletedAt,
                DurationMs  = (long)s.Duration.TotalMilliseconds,
            }).ToList(),
        };
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    public class WorkflowRunRequest
    {
        /// <summary>Name of the workflow to run: triage | daily-briefing | auto-close</summary>
        public string WorkflowName { get; set; } = string.Empty;

        /// <summary>Free-text input passed to the workflow's entry step.</summary>
        public string Input { get; set; } = string.Empty;
    }

    public class WorkflowExecutionResponse
    {
        public string ExecutionId { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Input { get; set; } = string.Empty;
        public string? FinalOutput { get; set; }
        public string? Error { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? DurationMs { get; set; }
        public List<WorkflowStepResponse> Steps { get; set; } = [];
    }

    public class WorkflowStepResponse
    {
        public string StepName { get; set; } = string.Empty;
        public string StepType { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
        public string? BranchTaken { get; set; }
        public DateTime CompletedAt { get; set; }
        public long DurationMs { get; set; }
    }
}
