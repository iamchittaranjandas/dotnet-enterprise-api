using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Application.Interfaces;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetAllTasks
{
    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, Result<List<TaskResponse>>>
    {
        private readonly ITaskRepository _taskRepository;

        public GetAllTasksQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Result<List<TaskResponse>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _taskRepository.GetAllAsync(1, int.MaxValue);

            var response = tasks.Select(t => new TaskResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedDate = t.CreatedDate
            }).ToList();

            return Result<List<TaskResponse>>.Success(response, "Tasks retrieved successfully");
        }
    }
}
