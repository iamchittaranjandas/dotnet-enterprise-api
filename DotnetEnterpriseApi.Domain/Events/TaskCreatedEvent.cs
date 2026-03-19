using DotnetEnterpriseApi.Domain.Common;

namespace DotnetEnterpriseApi.Domain.Events
{
    public class TaskCreatedEvent : IDomainEvent
    {
        public int TaskId { get; }
        public string Title { get; }
        public DateTime OccurredOn { get; }

        public TaskCreatedEvent(int taskId, string title)
        {
            TaskId = taskId;
            Title = title;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
