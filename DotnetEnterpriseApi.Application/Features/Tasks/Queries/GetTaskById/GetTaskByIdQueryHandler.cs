using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Application.Interfaces;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetTaskById
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskResponse>>
    {
        private readonly ITaskRepository _taskRepository;

        public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Result<TaskResponse>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _taskRepository.GetByIdAsync(request.Id);

            if (task == null)
            {
                return Result<TaskResponse>.Failure("Task not found");
            }

            var response = new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedDate = task.CreatedDate
            };

            return Result<TaskResponse>.Success(response, "Task retrieved successfully");
        }
    }
}
