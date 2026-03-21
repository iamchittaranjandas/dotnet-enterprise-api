using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.EntityFramework
{
    public class EfRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public EfRefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            return refreshToken;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens.AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
            }
        }
    }
}
