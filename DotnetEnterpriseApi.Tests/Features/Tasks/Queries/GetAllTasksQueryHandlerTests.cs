using AutoMapper;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Tests.Common;
using DotnetEnterpriseApi.Application.Features.Tasks.Queries.GetAllTasks;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DotnetEnterpriseApi.Tests.Features.Tasks.Queries
{
    public class GetAllTasksQueryHandlerTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly IMapper _mapper;
        private readonly GetAllTasksQueryHandler _handler;

        public GetAllTasksQueryHandlerTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _mapper = TestMapperFactory.Create();
            _handler = new GetAllTasksQueryHandler(_taskRepositoryMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_WithTasks_ReturnsPagedResult()
        {
            var query = new GetAllTasksQuery { Cursor = null, PageSize = 2 };
            var tasks = new List<TaskItem>
            {
                new() { Id = 1, Title = "Task 1", Description = "Desc 1" },
                new() { Id = 2, Title = "Task 2", Description = "Desc 2" }
            };

            _taskRepositoryMock.Setup(x => x.GetAllAsync(null, 3)).ReturnsAsync(tasks);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Items.Should().HaveCount(2);
            result.Data.HasNextPage.Should().BeFalse();
            result.Data.NextCursor.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithMoreThanPageSize_ReturnsHasNextPage()
        {
            var query = new GetAllTasksQuery { Cursor = null, PageSize = 2 };
            var tasks = new List<TaskItem>
            {
                new() { Id = 1, Title = "Task 1", Description = "Desc 1" },
                new() { Id = 2, Title = "Task 2", Description = "Desc 2" },
                new() { Id = 3, Title = "Task 3", Description = "Desc 3" }
            };

            _taskRepositoryMock.Setup(x => x.GetAllAsync(null, 3)).ReturnsAsync(tasks);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Items.Should().HaveCount(2);
            result.Data.HasNextPage.Should().BeTrue();
            result.Data.NextCursor.Should().Be(2);
        }

        [Fact]
        public async Task Handle_WithCursor_PassesCursorToRepository()
        {
            var query = new GetAllTasksQuery { Cursor = 5, PageSize = 10 };
            _taskRepositoryMock.Setup(x => x.GetAllAsync(5, 11)).ReturnsAsync(new List<TaskItem>());

            await _handler.Handle(query, CancellationToken.None);

            _taskRepositoryMock.Verify(x => x.GetAllAsync(5, 11), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyItems()
        {
            var query = new GetAllTasksQuery { Cursor = null, PageSize = 10 };
            _taskRepositoryMock.Setup(x => x.GetAllAsync(null, 11)).ReturnsAsync(new List<TaskItem>());

            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Items.Should().BeEmpty();
            result.Data.HasNextPage.Should().BeFalse();
        }
    }
}
