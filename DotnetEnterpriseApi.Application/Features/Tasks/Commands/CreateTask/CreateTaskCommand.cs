using DotnetEnterpriseApi.Application.Common.Models;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask
{
    public class CreateTaskCommand : IRequest<Result<TaskResponse>>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
