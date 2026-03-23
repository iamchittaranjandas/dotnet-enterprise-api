using AutoMapper;
using DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetTaskById;
using DotnetEnterpriseApi.Tests.Common;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DotnetEnterpriseApi.Tests.Features.Tasks.Queries
{
    public class GetTaskByIdQueryHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetTaskByIdQueryHandler _handler;

        public GetTaskByIdQueryHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _mapper = TestMapperFactory.Create();
            _handler = new GetTaskByIdQueryHandler(_taskRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ExistingTask_ReturnsSuccessWithTask()
        {
            var task = new TaskItem
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow
            };

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(task);

            var result = await _handler.Handle(new GetTaskByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Id.Should().Be(1);
            result.Data.Title.Should().Be("Test Task");
            result.Data.Description.Should().Be("Test Description");
            result.Message.Should().Be("Task retrieved successfully");
        }

        [Fact]
        public async Task Handle_NonExistingTask_ReturnsFailure()
        {
            _taskRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((TaskItem?)null);

            var result = await _handler.Handle(new GetTaskByIdQuery { Id = 999 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Task not found");
            result.Data.Should().BeNull();
        }
    }
}
