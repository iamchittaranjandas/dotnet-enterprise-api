using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetAllTasks
{
    public class GetAllTasksQuery : IRequest<Result<CursorPagedResult<TaskResponse>>>
    {
        public int? Cursor { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
