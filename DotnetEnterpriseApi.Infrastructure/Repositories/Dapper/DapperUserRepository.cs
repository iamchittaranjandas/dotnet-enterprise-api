using System.Data;
using Dapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Dapper
{
    public class DapperUserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly ISqlDialect _dialect;

        public DapperUserRepository(ISqlConnectionFactory connectionFactory, ISqlDialect dialect)
        {
            _connectionFactory = connectionFactory;
            _dialect = dialect;
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            var sql = _dialect.FormatSql("SELECT Id, UserName, Email, PasswordHash, Role FROM Users WHERE Id = @Id");

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AppUser>(sql, new { Id = id });
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var sql = _dialect.FormatSql("SELECT COUNT(1) FROM Users WHERE Email = @Email");

            using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });

            return count > 0;
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            var sql = _dialect.FormatSql("SELECT Id, UserName, Email, PasswordHash, Role FROM Users WHERE Email = @Email");

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AppUser>(sql, new { Email = email });
        }

        public async Task<AppUser> AddAsync(AppUser user)
        {
            var sql = _dialect.FormatSql(_dialect.InsertReturningId(
                "Users",
                "UserName, Email, PasswordHash, Role",
                "@UserName, @Email, @PasswordHash, @Role"));

            using var connection = _connectionFactory.CreateConnection();

            if (_dialect.RequiresOutputParameterForInsert)
            {
                var parameters = new DynamicParameters();
                parameters.Add("UserName", user.UserName);
                parameters.Add("Email", user.Email);
                parameters.Add("PasswordHash", user.PasswordHash);
                parameters.Add("Role", user.Role);
                parameters.Add("NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await connection.ExecuteAsync(sql, parameters);
                user.Id = parameters.Get<int>("NewId");
            }
            else
            {
                var result = await connection.ExecuteScalarAsync<object>(sql, new
                {
                    user.UserName,
                    user.Email,
                    user.PasswordHash,
                    user.Role
                });
                user.Id = Convert.ToInt32(result);
            }

            return user;
        }
    }
}
