using AutoMapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Features.Authentication.Commands.Register;
using DotnetEnterpriseApi.Tests.Common;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DotnetEnterpriseApi.Tests.Features.Authentication.Commands
{
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly RegisterCommandHandler _handler;

        public RegisterCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapper = TestMapperFactory.Create();
            _handler = new RegisterCommandHandler(_userRepositoryMock.Object, _unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_NewUser_ReturnsSuccessWithRegisterResponse()
        {
            var command = new RegisterCommand
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password1"
            };

            _userRepositoryMock.Setup(x => x.UserExistsAsync("test@example.com")).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<AppUser>()))
                .ReturnsAsync((AppUser u) =>
                {
                    u.Id = 1;
                    return u;
                });
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.UserName.Should().Be("testuser");
            result.Data.Email.Should().Be("test@example.com");
            result.Data.Message.Should().Be("User registered successfully");
        }

        [Fact]
        public async Task Handle_ExistingUser_ReturnsFailure()
        {
            var command = new RegisterCommand
            {
                UserName = "testuser",
                Email = "existing@example.com",
                Password = "Password1"
            };

            _userRepositoryMock.Setup(x => x.UserExistsAsync("existing@example.com")).ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("User with this email already exists");
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AppUser>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NewUser_HashesPassword()
        {
            var command = new RegisterCommand
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password1"
            };

            AppUser? capturedUser = null;
            _userRepositoryMock.Setup(x => x.UserExistsAsync("test@example.com")).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<AppUser>()))
                .Callback<AppUser>(u => capturedUser = u)
                .ReturnsAsync((AppUser u) => u);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            capturedUser.Should().NotBeNull();
            capturedUser!.PasswordHash.Should().NotBe("Password1");
            BCrypt.Net.BCrypt.Verify("Password1", capturedUser.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NewUser_SetsRoleToUser()
        {
            var command = new RegisterCommand
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Password1"
            };

            AppUser? capturedUser = null;
            _userRepositoryMock.Setup(x => x.UserExistsAsync("test@example.com")).ReturnsAsync(false);
            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<AppUser>()))
                .Callback<AppUser>(u => capturedUser = u)
                .ReturnsAsync((AppUser u) => u);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            capturedUser!.Role.Should().Be("User");
        }
    }
}
