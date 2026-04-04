using AutoMapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.UpdateTask;
using DotnetEnterpriseApi.Tests.Common;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;

namespace DotnetEnterpriseApi.Tests.Features.Tasks.Commands
{
    public class UpdateTaskCommandHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UpdateTaskCommandHandler _handler;

        public UpdateTaskCommandHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapper = TestMapperFactory.Create();
            _mediatorMock = new Mock<IMediator>();
            _handler = new UpdateTaskCommandHandler(_taskRepositoryMock.Object, _unitOfWorkMock.Object, _mapper, _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingTask_ReturnsSuccessWithUpdatedData()
        {
            var existingTask = new TaskItem { Id = 1, Title = "Old", Description = "Old Desc", IsCompleted = false };
            var command = new UpdateTaskCommand { Id = 1, Title = "Updated", Description = "Updated Desc", IsCompleted = true };

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingTask);
            _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Title.Should().Be("Updated");
            result.Data.Description.Should().Be("Updated Desc");
            result.Data.IsCompleted.Should().BeTrue();
            result.Message.Should().Be("Task updated successfully");
        }

        [Fact]
        public async Task Handle_NonExistingTask_ReturnsFailure()
        {
            var command = new UpdateTaskCommand { Id = 999, Title = "Test", Description = "Desc" };

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((TaskItem?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Task not found");
            _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskItem>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingTask_CallsUpdateAndSaveChanges()
        {
            var existingTask = new TaskItem { Id = 1, Title = "Old", Description = "Old" };
            var command = new UpdateTaskCommand { Id = 1, Title = "New", Description = "New", IsCompleted = true };

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existingTask);
            _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            _taskRepositoryMock.Verify(x => x.UpdateAsync(It.Is<TaskItem>(t =>
                t.Title == "New" && t.Description == "New" && t.IsCompleted)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
