using AutoMapper;
using DotnetEnterpriseApi.Application.Common.Mappings;
using DotnetEnterpriseApi.Application.Features.Authentication.Commands.Register;
using DotnetEnterpriseApi.Application.Features.Tasks.Commands.CreateTask;
using DotnetEnterpriseApi.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetEnterpriseApi.Tests.Common
{
    public class MappingProfileTests
    {
        private readonly IMapper _mapper;

        public MappingProfileTests()
        {
            _mapper = TestMapperFactory.Create();
        }

        [Fact]
        public void Map_AppUser_To_RegisterResponse_MessageIsDefault()
        {
            var user = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User"
            };

            var response = _mapper.Map<RegisterResponse>(user);

            response.Message.Should().Be(string.Empty);
        }

        [Fact]
        public void Map_TaskItem_To_TaskResponse()
        {
            var taskItem = new TaskItem
            {
                Id = 1,
                Title = "Test",
                Description = "Desc",
                IsCompleted = true,
                CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var response = _mapper.Map<TaskResponse>(taskItem);

            response.Id.Should().Be(1);
            response.Title.Should().Be("Test");
            response.Description.Should().Be("Desc");
            response.IsCompleted.Should().BeTrue();
            response.CreatedDate.Should().Be(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void Map_AppUser_To_RegisterResponse()
        {
            var user = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User"
            };

            var response = _mapper.Map<RegisterResponse>(user);

            response.Id.Should().Be(1);
            response.UserName.Should().Be("testuser");
            response.Email.Should().Be("test@example.com");
        }
    }
}
