using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.DeleteTask;
using DotnetEnterpriseApi.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace DotnetEnterpriseApi.Tests.Features.Tasks.Commands
{
    public class DeleteTaskCommandHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DeleteTaskCommandHandler _handler;

        public DeleteTaskCommandHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new DeleteTaskCommandHandler(_taskRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingTask_ReturnsSuccess()
        {
            var command = new DeleteTaskCommand { Id = 1 };
            _taskRepositoryMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Task deleted successfully");
        }

        [Fact]
        public async Task Handle_NonExistingTask_ReturnsFailure()
        {
            var command = new DeleteTaskCommand { Id = 999 };
            _taskRepositoryMock.Setup(x => x.DeleteAsync(999)).ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Task not found");
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingTask_CallsDeleteAndSaveChanges()
        {
            var command = new DeleteTaskCommand { Id = 5 };
            _taskRepositoryMock.Setup(x => x.DeleteAsync(5)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            _taskRepositoryMock.Verify(x => x.DeleteAsync(5), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
