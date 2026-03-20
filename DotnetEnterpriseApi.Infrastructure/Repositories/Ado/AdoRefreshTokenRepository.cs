using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Ado
{
    public class AdoRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public AdoRefreshTokenRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            const string sql = @"
                INSERT INTO RefreshTokens (Token, UserId, ExpiresAt, CreatedAt, IsRevoked)
                OUTPUT INSERTED.Id
                VALUES (@Token, @UserId, @ExpiresAt, @CreatedAt, @IsRevoked)";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Token", refreshToken.Token);
            command.Parameters.AddWithValue("@UserId", refreshToken.UserId);
            command.Parameters.AddWithValue("@ExpiresAt", refreshToken.ExpiresAt);
            command.Parameters.AddWithValue("@CreatedAt", refreshToken.CreatedAt);
            command.Parameters.AddWithValue("@IsRevoked", refreshToken.IsRevoked);

            var id = (int)(await command.ExecuteScalarAsync())!;
            refreshToken.Id = id;
            return refreshToken;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            const string sql = "SELECT Id, Token, UserId, ExpiresAt, CreatedAt, IsRevoked FROM RefreshTokens WHERE Token = @Token";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Token", token);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new RefreshToken
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Token = reader.GetString(reader.GetOrdinal("Token")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                IsRevoked = reader.GetBoolean(reader.GetOrdinal("IsRevoked"))
            };
        }

        public async Task RevokeAsync(string token)
        {
            const string sql = "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Token", token);
            await command.ExecuteNonQueryAsync();
        }
    }
}
