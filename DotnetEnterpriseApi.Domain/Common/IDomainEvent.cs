using MediatR;

namespace DotnetEnterpriseApi.Domain.Common
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }
}
