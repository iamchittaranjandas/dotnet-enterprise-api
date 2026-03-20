using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
    {
        private readonly ITaskRepository _taskRepository;

        public CreateTaskCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _taskRepository.AddAsync(task);

            var response = new TaskResponse
            {
                Id = created.Id,
                Title = created.Title,
                Description = created.Description,
                IsCompleted = created.IsCompleted,
                CreatedDate = created.CreatedDate
            };

            return Result<TaskResponse>.Success(response, "Task created successfully");
        }
    }
}
