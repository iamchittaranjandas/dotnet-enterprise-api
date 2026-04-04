using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DotnetEnterpriseApi.Application.Features.Tasks.EventHandlers
{
    public class TaskEmbeddingHandler :
        INotificationHandler<TaskCreatedEvent>,
        INotificationHandler<TaskUpdatedEvent>
    {
        private readonly ITaskRagService _ragService;
        private readonly ILogger<TaskEmbeddingHandler> _logger;

        public TaskEmbeddingHandler(ITaskRagService ragService, ILogger<TaskEmbeddingHandler> logger)
        {
            _ragService = ragService;
            _logger = logger;
        }

        public async Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
        {
            await UpsertAsync(notification.TaskId, notification.Title, notification.Description, notification.IsCompleted, cancellationToken);
        }

        public async Task Handle(TaskUpdatedEvent notification, CancellationToken cancellationToken)
        {
            await UpsertAsync(notification.TaskId, notification.Title, notification.Description, notification.IsCompleted, cancellationToken);
        }

        private async Task UpsertAsync(int taskId, string title, string description, bool isCompleted, CancellationToken cancellationToken)
        {
            try
            {
                var task = new TaskItem
                {
                    Id = taskId,
                    Title = title,
                    Description = description,
                    IsCompleted = isCompleted,
                };

                await _ragService.UpsertTaskEmbeddingAsync(task, cancellationToken);
                _logger.LogDebug("RAG: Upserted embedding for task {TaskId}", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RAG: Failed to upsert embedding for task {TaskId}", taskId);
            }
        }
    }
}
