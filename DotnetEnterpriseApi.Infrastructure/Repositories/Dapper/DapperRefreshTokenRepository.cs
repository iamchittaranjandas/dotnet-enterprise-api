using System.Data;
using Dapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Infrastructure.Repositories.Queries;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Dapper
{
    public class DapperRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly ISqlDialect _dialect;

        public DapperRefreshTokenRepository(ISqlConnectionFactory connectionFactory, ISqlDialect dialect)
        {
            _connectionFactory = connectionFactory;
            _dialect = dialect;
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            var sql = RefreshTokenQueries.Insert(_dialect);

            using var connection = _connectionFactory.CreateConnection();

            if (_dialect.RequiresOutputParameterForInsert)
            {
                var parameters = new DynamicParameters();
                parameters.Add("Token", refreshToken.Token);
                parameters.Add("UserId", refreshToken.UserId);
                parameters.Add("ExpiresAt", refreshToken.ExpiresAt);
                parameters.Add("CreatedAt", refreshToken.CreatedAt);
                parameters.Add("IsRevoked", refreshToken.IsRevoked);
                parameters.Add("NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await connection.ExecuteAsync(sql, parameters);
                refreshToken.Id = parameters.Get<int>("NewId");
            }
            else
            {
                var result = await connection.ExecuteScalarAsync<object>(sql, new
                {
                    refreshToken.Token,
                    refreshToken.UserId,
                    refreshToken.ExpiresAt,
                    refreshToken.CreatedAt,
                    refreshToken.IsRevoked
                });
                refreshToken.Id = Convert.ToInt32(result);
            }

            return refreshToken;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            var sql = RefreshTokenQueries.GetByToken(_dialect);

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task RevokeAsync(string token)
        {
            var sql = RefreshTokenQueries.Revoke(_dialect);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { Token = token, IsRevoked = true });
        }
    }
}
