using DotnetEnterpriseApi.Domain.Common;

namespace DotnetEnterpriseApi.Domain.Events
{
    public class UserRegisteredEvent : IDomainEvent
    {
        public int UserId { get; }
        public string Email { get; }
        public DateTime OccurredOn { get; }

        public UserRegisteredEvent(int userId, string email)
        {
            UserId = userId;
            Email = email;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
