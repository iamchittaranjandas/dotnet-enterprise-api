using System.ComponentModel;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Application.Features.Tasks.AgentTools
{
    /// <summary>
    /// Write tools for the MutationAgent.
    /// All operations persist to the database and trigger domain events
    /// (which in turn update the RAG vector store via TaskEmbeddingHandler).
    /// </summary>
    public class MutationAgentTools
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MutationAgentTools(ITaskRepository taskRepository, IUnitOfWork unitOfWork)
        {
            _taskRepository = taskRepository;
            _unitOfWork     = unitOfWork;
        }

        [Description("Creates a new task with the given title and description.")]
        public async Task<string> CreateTaskAsync(
            [Description("Short, descriptive title of the task.")] string title,
            [Description("Detailed description of what the task involves.")] string description)
        {
            var task = new TaskItem
            {
                Title       = title,
                Description = description,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow,
            };

            var created = await _taskRepository.AddAsync(task);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return $"✅ Task created. ID: {created.Id}, Title: \"{created.Title}\"";
        }

        [Description("Marks an existing task as completed or reverts it to pending.")]
        public async Task<string> UpdateTaskCompletionAsync(
            [Description("The ID of the task to update.")] int id,
            [Description("true = mark completed; false = mark pending.")] bool isCompleted)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task is null)
                return $"❌ Task with ID {id} was not found.";

            task.IsCompleted = isCompleted;
            await _taskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return $"✅ Task ID {id} marked as {(isCompleted ? "completed" : "pending")}.";
        }

        [Description("Updates the title and/or description of an existing task.")]
        public async Task<string> UpdateTaskDetailsAsync(
            [Description("The ID of the task to update.")] int id,
            [Description("New title (leave blank to keep existing).")] string? title = null,
            [Description("New description (leave blank to keep existing).")] string? description = null)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task is null)
                return $"❌ Task with ID {id} was not found.";

            if (!string.IsNullOrWhiteSpace(title))
                task.Title = title;
            if (!string.IsNullOrWhiteSpace(description))
                task.Description = description;

            await _taskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return $"✅ Task ID {id} updated. Title: \"{task.Title}\"";
        }

        [Description("Permanently deletes a task by its ID. This cannot be undone.")]
        public async Task<string> DeleteTaskAsync(
            [Description("The ID of the task to delete.")] int id)
        {
            var deleted = await _taskRepository.DeleteAsync(id);
            if (!deleted)
                return $"❌ Task with ID {id} was not found or could not be deleted.";

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            return $"✅ Task ID {id} has been permanently deleted.";
        }
    }
}
