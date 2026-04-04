using System.ComponentModel;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Application.Features.Tasks.AgentTools
{
    /// <summary>
    /// Provides task management tools for the Microsoft Agent Framework AI agent.
    /// Methods are registered via AIFunctionFactory.Create and exposed to the LLM as callable tools.
    /// </summary>
    public class TaskAgentTools
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TaskAgentTools(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
        {
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
        }

        [Description("Lists tasks with optional cursor-based pagination. Returns a page of tasks.")]
        public async Task<string> ListTasksAsync(
            [Description("Optional cursor (task ID) to start listing from. Omit to start from beginning.")] int? cursor,
            [Description("Number of tasks to return per page. Default is 10.")] int pageSize = 10)
        {
            var tasks = await _taskRepository.GetAllAsync(cursor, pageSize);
            if (tasks.Count == 0)
                return "No tasks found.";

            var lines = tasks.Select(t =>
                $"[ID:{t.Id}] {t.Title} — {(t.IsCompleted ? "Completed" : "Pending")} | {t.Description}");
            return string.Join("\n", lines);
        }

        [Description("Gets a single task by its ID.")]
        public async Task<string> GetTaskByIdAsync(
            [Description("The unique ID of the task to retrieve.")] int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task is null)
                return $"Task with ID {id} was not found.";

            return $"[ID:{task.Id}] {task.Title}\nStatus: {(task.IsCompleted ? "Completed" : "Pending")}\nDescription: {task.Description}\nCreated: {task.CreatedDate:yyyy-MM-dd HH:mm} UTC";
        }

        [Description("Creates a new task with the given title and description.")]
        public async Task<string> CreateTaskAsync(
            [Description("The title of the new task.")] string title,
            [Description("A detailed description of what the task involves.")] string description)
        {
            var task = new TaskItem
            {
                Title = title,
                Description = description,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _taskRepository.AddAsync(task);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return $"Task created successfully. ID: {created.Id}, Title: \"{created.Title}\"";
        }

        [Description("Marks an existing task as completed or incomplete.")]
        public async Task<string> UpdateTaskCompletionAsync(
            [Description("The ID of the task to update.")] int id,
            [Description("Set to true to mark as completed, false to mark as incomplete.")] bool isCompleted)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task is null)
                return $"Task with ID {id} was not found.";

            task.IsCompleted = isCompleted;
            var updated = await _taskRepository.UpdateAsync(task);
            if (!updated)
                return $"Failed to update task with ID {id}.";

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            return $"Task ID {id} marked as {(isCompleted ? "completed" : "incomplete")}.";
        }

        [Description("Deletes a task permanently by its ID.")]
        public async Task<string> DeleteTaskAsync(
            [Description("The ID of the task to delete.")] int id)
        {
            var deleted = await _taskRepository.DeleteAsync(id);
            if (!deleted)
                return $"Task with ID {id} was not found or could not be deleted.";

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            return $"Task ID {id} has been deleted.";
        }
    }
}
