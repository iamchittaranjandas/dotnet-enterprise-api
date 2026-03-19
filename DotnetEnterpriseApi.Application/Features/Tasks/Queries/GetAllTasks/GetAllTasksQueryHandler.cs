using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetAllTasks
{
    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, Result<List<TaskResponse>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllTasksQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<TaskResponse>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _context.Tasks
                .Select(t => new TaskResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedDate = t.CreatedDate
                })
                .ToListAsync(cancellationToken);

            return Result<List<TaskResponse>>.Success(tasks, "Tasks retrieved successfully");
        }
    }
}
