using Dapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Dapper
{
    public class DapperRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public DapperRefreshTokenRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            const string sql = @"
                INSERT INTO RefreshTokens (Token, UserId, ExpiresAt, CreatedAt, IsRevoked)
                OUTPUT INSERTED.Id
                VALUES (@Token, @UserId, @ExpiresAt, @CreatedAt, @IsRevoked)";

            using var connection = _connectionFactory.CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                refreshToken.Token,
                refreshToken.UserId,
                refreshToken.ExpiresAt,
                refreshToken.CreatedAt,
                refreshToken.IsRevoked
            });

            refreshToken.Id = id;
            return refreshToken;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            const string sql = "SELECT Id, Token, UserId, ExpiresAt, CreatedAt, IsRevoked FROM RefreshTokens WHERE Token = @Token";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task RevokeAsync(string token)
        {
            const string sql = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { Token = token });
        }
    }
}
