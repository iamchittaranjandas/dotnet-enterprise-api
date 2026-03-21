using System.Data;
using System.Data.Common;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Infrastructure.Repositories.Queries;

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
            var sql = RefreshTokenQueries.Insert(_dialect);

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, _dialect.FormatSql("@Token"), refreshToken.Token);
            AddParameter(command, _dialect.FormatSql("@UserId"), refreshToken.UserId);
            AddParameter(command, _dialect.FormatSql("@ExpiresAt"), refreshToken.ExpiresAt);
            AddParameter(command, _dialect.FormatSql("@CreatedAt"), refreshToken.CreatedAt);
            AddParameter(command, _dialect.FormatSql("@IsRevoked"), refreshToken.IsRevoked);

            if (_dialect.RequiresOutputParameterForInsert)
            {
                var outParam = command.CreateParameter();
                outParam.ParameterName = "NewId";
                outParam.DbType = DbType.Int32;
                outParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(outParam);
                await command.ExecuteNonQueryAsync();
                refreshToken.Id = Convert.ToInt32(outParam.Value);
            }
            else
            {
                refreshToken.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            return refreshToken;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            var sql = RefreshTokenQueries.GetByToken(_dialect);

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, _dialect.FormatSql("@Token"), token);

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
            var sql = RefreshTokenQueries.Revoke(_dialect);

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, _dialect.FormatSql("@Token"), token);
            AddParameter(command, _dialect.FormatSql("@IsRevoked"), true);
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
