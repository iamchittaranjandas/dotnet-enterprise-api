using DotnetEnterpriseApi.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DotnetEnterpriseApi.Application.Features.Tasks.EventHandlers
{
    public class TaskCreatedEventHandler : INotificationHandler<TaskCreatedEvent>
    {
        private readonly ILogger<TaskCreatedEventHandler> _logger;

        public TaskCreatedEventHandler(ILogger<TaskCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event: Task Created - TaskId: {TaskId}, Title: {Title}", 
                notification.TaskId, notification.Title);

            return Task.CompletedTask;
        }
    }
}
