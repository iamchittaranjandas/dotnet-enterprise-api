using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Commands.UpdateTask
{
    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskResponse>>
    {
        private readonly ITaskRepository _taskRepository;

        public UpdateTaskCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Result<TaskResponse>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _taskRepository.GetByIdAsync(request.Id);

            if (task == null)
            {
                return Result<TaskResponse>.Failure("Task not found");
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.IsCompleted = request.IsCompleted;

            await _taskRepository.UpdateAsync(task);

            var response = new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                CreatedDate = task.CreatedDate
            };

            return Result<TaskResponse>.Success(response, "Task updated successfully");
        }
    }
}
