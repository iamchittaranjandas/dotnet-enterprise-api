using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Features.Authentication.Commands.RefreshToken;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace DotnetEnterpriseApi.Tests.Features.Authentication.Commands
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IConfiguration _configuration;
        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTests()
        {
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsMySuperSecretKeyHere_MinimumLength32Characters!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();

            _handler = new RefreshTokenCommandHandler(
                _refreshTokenRepositoryMock.Object,
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _configuration);
        }

        [Fact]
        public async Task Handle_ValidActiveToken_ReturnsNewTokens()
        {
            var existingToken = new Domain.Entities.RefreshToken
            {
                Id = 1,
                Token = "existing-token",
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsRevoked = false
            };

            var user = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User"
            };

            var command = new RefreshTokenCommand { Token = "existing-token" };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("existing-token")).ReturnsAsync(existingToken);
            _refreshTokenRepositoryMock.Setup(x => x.RevokeAsync("existing-token")).Returns(Task.CompletedTask);
            _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Domain.Entities.RefreshToken>()))
                .ReturnsAsync((Domain.Entities.RefreshToken t) => t);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Token.Should().NotBeNullOrEmpty();
            result.Data.RefreshToken.Should().NotBeNullOrEmpty();
            result.Data.UserName.Should().Be("testuser");
            result.Data.Email.Should().Be("test@example.com");
            result.Message.Should().Be("Token refreshed successfully");
        }

        [Fact]
        public async Task Handle_NullToken_ReturnsFailure()
        {
            var command = new RefreshTokenCommand { Token = "nonexistent" };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("nonexistent"))
                .ReturnsAsync((Domain.Entities.RefreshToken?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid or expired refresh token");
        }

        [Fact]
        public async Task Handle_RevokedToken_ReturnsFailure()
        {
            var revokedToken = new Domain.Entities.RefreshToken
            {
                Id = 1,
                Token = "revoked-token",
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsRevoked = true
            };

            var command = new RefreshTokenCommand { Token = "revoked-token" };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("revoked-token")).ReturnsAsync(revokedToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid or expired refresh token");
        }

        [Fact]
        public async Task Handle_ExpiredToken_ReturnsFailure()
        {
            var expiredToken = new Domain.Entities.RefreshToken
            {
                Id = 1,
                Token = "expired-token",
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                IsRevoked = false
            };

            var command = new RefreshTokenCommand { Token = "expired-token" };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("expired-token")).ReturnsAsync(expiredToken);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Invalid or expired refresh token");
        }

        [Fact]
        public async Task Handle_ValidToken_UserNotFound_ReturnsFailure()
        {
            var existingToken = new Domain.Entities.RefreshToken
            {
                Id = 1,
                Token = "valid-token",
                UserId = 999,
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsRevoked = false
            };

            var command = new RefreshTokenCommand { Token = "valid-token" };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("valid-token")).ReturnsAsync(existingToken);
            _refreshTokenRepositoryMock.Setup(x => x.RevokeAsync("valid-token")).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((AppUser?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("User not found");
        }

        [Fact]
        public async Task Handle_ValidToken_RevokesOldAndCreatesNew()
        {
            var existingToken = new Domain.Entities.RefreshToken
            {
                Id = 1,
                Token = "old-token",
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsRevoked = false
            };

            var user = new AppUser { Id = 1, UserName = "test", Email = "test@test.com", PasswordHash = "h", Role = "User" };
            var command = new RefreshTokenCommand { Token = "old-token" };

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("old-token")).ReturnsAsync(existingToken);
            _refreshTokenRepositoryMock.Setup(x => x.RevokeAsync("old-token")).Returns(Task.CompletedTask);
            _refreshTokenRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Domain.Entities.RefreshToken>()))
                .ReturnsAsync((Domain.Entities.RefreshToken t) => t);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            _refreshTokenRepositoryMock.Verify(x => x.RevokeAsync("old-token"), Times.Once);
            _refreshTokenRepositoryMock.Verify(x => x.CreateAsync(It.Is<Domain.Entities.RefreshToken>(t =>
                t.UserId == 1 && t.Token != "old-token")), Times.Once);
        }
    }
}
