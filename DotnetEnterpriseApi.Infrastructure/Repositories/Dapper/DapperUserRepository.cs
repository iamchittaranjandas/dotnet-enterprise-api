using Dapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Dapper
{
    public class DapperUserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public DapperUserRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            const string sql = "SELECT Id, UserName, Email, PasswordHash, Role FROM Users WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AppUser>(sql, new { Id = id });
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

            using var connection = _connectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });

            return count > 0;
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            const string sql = "SELECT Id, UserName, Email, PasswordHash, Role FROM Users WHERE Email = @Email";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AppUser>(sql, new { Email = email });
        }

        public async Task<AppUser> AddAsync(AppUser user)
        {
            const string sql = @"
                INSERT INTO Users (UserName, Email, PasswordHash, Role)
                OUTPUT INSERTED.Id
                VALUES (@UserName, @Email, @PasswordHash, @Role)";

            using var connection = _connectionFactory.CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                user.UserName,
                user.Email,
                user.PasswordHash,
                user.Role
            });

            user.Id = id;
            return user;
        }
    }
}
