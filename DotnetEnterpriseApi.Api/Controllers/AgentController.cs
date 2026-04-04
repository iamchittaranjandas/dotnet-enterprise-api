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
    [Authorize]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly IChatClient _chatClient;
        private readonly TaskAgentTools _taskTools;
        private readonly ITaskRagService _ragService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<AgentController> _logger;

        public AgentController(
            IChatClient chatClient,
            TaskAgentTools taskTools,
            ITaskRagService ragService,
            ILoggerFactory loggerFactory)
        {
            _chatClient   = chatClient;
            _taskTools    = taskTools;
            _ragService   = ragService;
            _loggerFactory = loggerFactory;
            _logger       = loggerFactory.CreateLogger<AgentController>();
        }

        /// <summary>
        /// Send a natural-language message to the task management agent.
        /// The agent uses RAG to retrieve semantically relevant tasks as context,
        /// then calls tools to list, create, update, or delete tasks on your behalf.
        /// </summary>
        /// <param name="request">
        /// <b>message</b> — the user's natural-language input.<br/>
        /// <b>filterCompleted</b> (optional) — restrict RAG context to completed (<c>true</c>),
        /// pending (<c>false</c>), or all tasks (<c>null</c>, default).
        /// </param>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AgentChatRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Message cannot be empty." });

            var ragResult = await _ragService.RetrieveContextAsync(
                request.Message,
                topK: 5,
                filterCompleted: request.FilterCompleted,
                hybrid: request.Hybrid,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Agent: RAG returned {MatchCount} context item(s) for '{Message}'",
                ragResult.Matches.Count, request.Message);

            var instructions = ragResult.HasContext
                ? $"""
                  You are a task management assistant integrated into an enterprise API.
                  Help users manage their tasks by calling the appropriate tools.
                  Always confirm actions with the result from the tool.
                  Be concise and professional in your responses.

                  The following tasks are semantically relevant to the user's message:
                  {ragResult.Context}

                  Use this context to inform your response, but rely on tools for mutations and current data.
                  """
                : """
                  You are a task management assistant integrated into an enterprise API.
                  Help users manage their tasks by calling the appropriate tools.
                  Always confirm actions with the result from the tool.
                  Be concise and professional in your responses.
                  """;

            var tools = new List<AITool>
            {
                AIFunctionFactory.Create(_taskTools.ListTasksAsync),
                AIFunctionFactory.Create(_taskTools.GetTaskByIdAsync),
                AIFunctionFactory.Create(_taskTools.CreateTaskAsync),
                AIFunctionFactory.Create(_taskTools.UpdateTaskCompletionAsync),
                AIFunctionFactory.Create(_taskTools.DeleteTaskAsync),
            };

            var agent = _chatClient.AsAIAgent(
                name: "TaskManagerAgent",
                instructions: instructions,
                description: "Manages tasks via natural language",
                tools: tools,
                loggerFactory: _loggerFactory);

            var session  = await agent.CreateSessionAsync(cancellationToken);
            var response = await agent.RunAsync(request.Message, session, cancellationToken: cancellationToken);

            return Ok(new AgentChatResponse
            {
                Reply         = response.ToString(),
                RagContext     = ragResult.Matches.Select(m => new RagContextItem
                {
                    TaskId      = m.TaskId,
                    Title       = m.Title,
                    IsCompleted = m.IsCompleted,
                    Score       = MathF.Round(m.Score, 4),
                }).ToList(),
            });
        }
    }

    public class AgentChatRequest
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Optional filter for RAG retrieval:
        /// <c>true</c> = completed tasks only, <c>false</c> = pending only, <c>null</c> = all (default).
        /// </summary>
        public bool? FilterCompleted { get; set; }

        /// <summary>
        /// When <c>true</c> (default) RAG uses hybrid search — BM25 keyword scoring
        /// combined with vector cosine similarity via Reciprocal Rank Fusion.
        /// Set to <c>false</c> for pure vector search.
        /// </summary>
        public bool Hybrid { get; set; } = true;
    }

    public class AgentChatResponse
    {
        public string Reply { get; set; } = string.Empty;

        /// <summary>
        /// The RAG context items injected into the agent's system prompt.
        /// Useful for debugging relevance and similarity scores.
        /// </summary>
        public IReadOnlyList<RagContextItem> RagContext { get; set; } = [];
    }

    public class RagContextItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public float Score { get; set; }
    }
}
