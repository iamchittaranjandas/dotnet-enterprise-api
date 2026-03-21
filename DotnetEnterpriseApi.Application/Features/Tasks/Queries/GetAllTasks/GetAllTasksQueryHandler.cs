using AutoMapper;
using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Application.Interfaces;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetAllTasks
{
    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, Result<CursorPagedResult<TaskResponse>>>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;

        public GetAllTasksQueryHandler(ITaskRepository taskRepository, IMapper mapper)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
        }

        public async Task<Result<CursorPagedResult<TaskResponse>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _taskRepository.GetAllAsync(request.Cursor, request.PageSize + 1);

            var hasNextPage = tasks.Count > request.PageSize;
            if (hasNextPage)
            {
                tasks = tasks.Take(request.PageSize).ToList();
            }

            var items = _mapper.Map<List<TaskResponse>>(tasks);

            var result = new CursorPagedResult<TaskResponse>
            {
                Items = items,
                HasNextPage = hasNextPage,
                NextCursor = hasNextPage ? tasks.Last().Id : null
            };

            return Result<CursorPagedResult<TaskResponse>>.Success(result, "Tasks retrieved successfully");
        }
    }
}
