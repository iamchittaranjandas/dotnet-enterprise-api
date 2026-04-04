using AutoMapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Common.Models;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Domain.Events;
using MediatR;

namespace DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask
{
    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateTaskCommandHandler(ITaskRepository taskRepository, IUnitOfWork unitOfWork, IMapper mapper, IMediator mediator)
        {
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mediator = mediator;
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(
                new TaskCreatedEvent(created.Id, created.Title, created.Description, created.IsCompleted),
                cancellationToken);

            var response = _mapper.Map<TaskResponse>(created);

            return Result<TaskResponse>.Success(response, "Task created successfully");
        }
    }
}
