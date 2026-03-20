using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Ado
{
    public class AdoUserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public AdoUserRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            var count = (int)(await command.ExecuteScalarAsync())!;
            return count > 0;
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            const string sql = "SELECT Id, UserName, Email, PasswordHash, Role FROM Users WHERE Email = @Email";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new AppUser
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role"))
            };
        }

        public async Task<AppUser> AddAsync(AppUser user)
        {
            const string sql = @"
                INSERT INTO Users (UserName, Email, PasswordHash, Role)
                OUTPUT INSERTED.Id
                VALUES (@UserName, @Email, @PasswordHash, @Role)";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Role", user.Role);

            var id = (int)(await command.ExecuteScalarAsync())!;
            user.Id = id;
            return user;
        }
    }
}
