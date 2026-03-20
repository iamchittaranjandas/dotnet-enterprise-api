using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Commands.DeleteTask
{
    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result>
    {
        private readonly ITaskRepository _taskRepository;

        public DeleteTaskCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _taskRepository.DeleteAsync(request.Id);

            if (!deleted)
            {
                return Result.Failure("Task not found");
            }

            return Result.Success("Task deleted successfully");
        }
    }
}
