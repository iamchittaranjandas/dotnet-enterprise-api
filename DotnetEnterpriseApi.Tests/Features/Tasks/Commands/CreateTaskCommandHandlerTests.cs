using AutoMapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Tests.Common;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DotnetEnterpriseApi.Tests.Features.Tasks.Commands
{
    public class CreateTaskCommandHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly CreateTaskCommandHandler _handler;

        public CreateTaskCommandHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapper = TestMapperFactory.Create();
            _handler = new CreateTaskCommandHandler(_taskRepositoryMock.Object, _unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessWithTaskResponse()
        {
            var command = new CreateTaskCommand { Title = "Test Task", Description = "Test Description" };

            _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>()))
                .ReturnsAsync((TaskItem t) =>
                {
                    t.Id = 1;
                    return t;
                });

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Title.Should().Be("Test Task");
            result.Data.Description.Should().Be("Test Description");
            result.Data.IsCompleted.Should().BeFalse();
            result.Message.Should().Be("Task created successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAddAsyncAndSaveChanges()
        {
            var command = new CreateTaskCommand { Title = "Test", Description = "Desc" };

            _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>()))
                .ReturnsAsync((TaskItem t) => t);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            _taskRepositoryMock.Verify(x => x.AddAsync(It.Is<TaskItem>(t =>
                t.Title == "Test" && t.Description == "Desc" && !t.IsCompleted)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsCreatedDateToUtcNow()
        {
            var command = new CreateTaskCommand { Title = "Test", Description = "Desc" };
            var beforeUtc = DateTime.UtcNow;

            _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>()))
                .ReturnsAsync((TaskItem t) => t);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Data!.CreatedDate.Should().BeOnOrAfter(beforeUtc);
            result.Data.CreatedDate.Should().BeOnOrBefore(DateTime.UtcNow);
        }
    }
}
