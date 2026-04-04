using DotnetEnterpriseApi.Domain.Common;

namespace DotnetEnterpriseApi.Domain.Events
{
    public class TaskUpdatedEvent : IDomainEvent
    {
        public int TaskId { get; }
        public string Title { get; }
        public string Description { get; }
        public bool IsCompleted { get; }
        public DateTime OccurredOn { get; }

        public TaskUpdatedEvent(int taskId, string title, string description, bool isCompleted)
        {
            TaskId = taskId;
            Title = title;
            Description = description;
            IsCompleted = isCompleted;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
