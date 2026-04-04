using System.ComponentModel;
using DotnetEnterpriseApi.Application.Interfaces;

namespace DotnetEnterpriseApi.Application.Features.Tasks.AgentTools
{
    /// <summary>
    /// Read-only tools for the QueryAgent.
    /// These tools never mutate state — they are safe to call for any read request.
    /// </summary>
    public class QueryAgentTools
    {
        private readonly ITaskRepository _taskRepository;

        public QueryAgentTools(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        [Description("Lists tasks with optional cursor-based pagination. Returns a formatted page of tasks.")]
        public async Task<string> ListTasksAsync(
            [Description("Optional cursor (task ID) to start listing from. Omit for the first page.")] int? cursor = null,
            [Description("Number of tasks per page. Default is 10.")] int pageSize = 10)
        {
            var tasks = await _taskRepository.GetAllAsync(cursor, pageSize);
            if (tasks.Count == 0)
                return "No tasks found.";

            var lines = tasks.Select(t =>
                $"[ID:{t.Id}] {t.Title} — {(t.IsCompleted ? "✅ Completed" : "⏳ Pending")} | {t.Description}");
            return string.Join("\n", lines);
        }

        [Description("Gets full details of a single task by its ID.")]
        public async Task<string> GetTaskByIdAsync(
            [Description("The unique ID of the task.")] int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task is null)
                return $"Task with ID {id} was not found.";

            return $"[ID:{task.Id}] {task.Title}\n" +
                   $"Status: {(task.IsCompleted ? "✅ Completed" : "⏳ Pending")}\n" +
                   $"Description: {task.Description}\n" +
                   $"Created: {task.CreatedDate:yyyy-MM-dd HH:mm} UTC";
        }

        [Description("Counts tasks, optionally filtered by completion status.")]
        public async Task<string> CountTasksAsync(
            [Description("Filter by status: 'completed', 'pending', or 'all' (default).")] string status = "all")
        {
            // Fetch a large page — sufficient for typical task counts
            var all = await _taskRepository.GetAllAsync(cursor: null, pageSize: 10_000);

            var count = status.ToLowerInvariant() switch
            {
                "completed" => all.Count(t => t.IsCompleted),
                "pending"   => all.Count(t => !t.IsCompleted),
                _           => all.Count,
            };

            return $"Total {status} tasks: {count}";
        }
    }
}
