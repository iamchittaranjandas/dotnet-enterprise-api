using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Commands.UpdateTask
{
    public class UpdateTaskCommand : IRequest<Result<TaskResponse>>
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}
