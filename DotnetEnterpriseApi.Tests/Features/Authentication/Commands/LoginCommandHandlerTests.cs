using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Features.Authentication.Commands.Login;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace DotnetEnterpriseApi.Tests.Features.Authentication.Commands
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IConfiguration _configuration;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsMySuperSecretKeyHere_MinimumLength32Characters!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

            _handler = new LoginCommandHandler(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _configuration);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsSuccessWithTokens()
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password1");
            var user = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = passwordHash,
                Role = "User"
            };

            var command = new LoginCommand { Email = "test@example.com", Password = "Password1" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync("test@example.com")).ReturnsAsync(user);
            _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync((RefreshToken t) => t);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Token.Should().NotBeNullOrEmpty();
            result.Data.RefreshToken.Should().NotBeNullOrEmpty();
            result.Data.UserName.Should().Be("testuser");
            result.Data.Email.Should().Be("test@example.com");
            result.Data.Role.Should().Be("User");
        }

        [Fact]
        public async Task Handle_InvalidEmail_ReturnsFailure()
        {
            var command = new LoginCommand { Email = "nonexistent@example.com", Password = "Password1" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync("nonexistent@example.com")).ReturnsAsync((AppUser?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid email or password");
        }

        [Fact]
        public async Task Handle_WrongPassword_ReturnsFailure()
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1");
            var user = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = passwordHash,
                Role = "User"
            };

            var command = new LoginCommand { Email = "test@example.com", Password = "WrongPassword1" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid email or password");
        }

        [Fact]
        public async Task Handle_ValidLogin_CreatesRefreshToken()
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password1");
            var user = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = passwordHash,
                Role = "User"
            };

            var command = new LoginCommand { Email = "test@example.com", Password = "Password1" };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync("test@example.com")).ReturnsAsync(user);
            _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync((RefreshToken t) => t);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            _refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.Is<RefreshToken>(t =>
                t.UserId == 1 && t.Token != null)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
