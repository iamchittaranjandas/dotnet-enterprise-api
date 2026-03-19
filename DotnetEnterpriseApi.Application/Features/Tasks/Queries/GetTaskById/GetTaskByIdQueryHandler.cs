using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetTaskById
{
    public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<TaskResponse>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _context.Tasks
                .Where(t => t.Id == request.Id)
                .Select(t => new TaskResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedDate = t.CreatedDate
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
            {
                return Result<TaskResponse>.Failure("Task not found");
            }

            return Result<TaskResponse>.Success(task, "Task retrieved successfully");
        }
    }
}
