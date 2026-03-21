using System.Data;
using System.Data.Common;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Ado
{
    public class AdoRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly ISqlDialect _dialect;

        public AdoRefreshTokenRepository(ISqlConnectionFactory connectionFactory, ISqlDialect dialect)
        {
            _connectionFactory = connectionFactory;
            _dialect = dialect;
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            var sql = _dialect.InsertReturningId(
                "RefreshTokens",
                "Token, UserId, ExpiresAt, CreatedAt, IsRevoked",
                "@Token, @UserId, @ExpiresAt, @CreatedAt, @IsRevoked");

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Token", refreshToken.Token);
            AddParameter(command, "@UserId", refreshToken.UserId);
            AddParameter(command, "@ExpiresAt", refreshToken.ExpiresAt);
            AddParameter(command, "@CreatedAt", refreshToken.CreatedAt);
            AddParameter(command, "@IsRevoked", refreshToken.IsRevoked);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            refreshToken.Id = id;
            return refreshToken;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            const string sql = "SELECT Id, Token, UserId, ExpiresAt, CreatedAt, IsRevoked FROM RefreshTokens WHERE Token = @Token";

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Token", token);

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
            const string sql = "UPDATE RefreshTokens SET IsRevoked = @IsRevoked WHERE Token = @Token";

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Token", token);
            AddParameter(command, "@IsRevoked", true);
            await command.ExecuteNonQueryAsync();
        }

        private static void AddParameter(IDbCommand command, string name, object? value)
        {
            var p = command.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }
    }
}
