using DotnetEnterpriseApi.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DotnetEnterpriseApi.Application.Features.Authentication.EventHandlers
{
    public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredEventHandler> _logger;

        public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event: User Registered - UserId: {UserId}, Email: {Email}", 
                notification.UserId, notification.Email);

            return Task.CompletedTask;
        }
    }
}
