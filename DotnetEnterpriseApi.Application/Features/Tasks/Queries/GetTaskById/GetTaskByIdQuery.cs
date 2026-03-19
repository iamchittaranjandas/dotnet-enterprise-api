using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetTaskById
{
    public class GetTaskByIdQuery : IRequest<Result<TaskResponse>>
    {
        public int Id { get; set; }
    }
}
