using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);

        Task<RefreshToken?> GetByTokenAsync(string token);

        Task RevokeAsync(string token);
    }
}
