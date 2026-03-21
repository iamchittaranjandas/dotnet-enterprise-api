using System.Data;
using System.Data.Common;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Ado
{
    public class AdoUserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly ISqlDialect _dialect;

        public AdoUserRepository(ISqlConnectionFactory connectionFactory, ISqlDialect dialect)
        {
            _connectionFactory = connectionFactory;
            _dialect = dialect;
        }

        public async Task<AppUser?> GetByIdAsync(int id)
        {
            const string sql = "SELECT Id, UserName, Email, PasswordHash, Role FROM Users WHERE Id = @Id";

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return MapUser(reader);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Email", email);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<AppUser?> GetByEmailAsync(string email)
        {
            const string sql = "SELECT Id, UserName, Email, PasswordHash, Role FROM Users WHERE Email = @Email";

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return MapUser(reader);
        }

        public async Task<AppUser> AddAsync(AppUser user)
        {
            var sql = _dialect.InsertReturningId(
                "Users",
                "UserName, Email, PasswordHash, Role",
                "@UserName, @Email, @PasswordHash, @Role");

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@UserName", user.UserName);
            AddParameter(command, "@Email", user.Email);
            AddParameter(command, "@PasswordHash", user.PasswordHash);
            AddParameter(command, "@Role", user.Role);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            user.Id = id;
            return user;
        }

        private static AppUser MapUser(IDataReader reader)
        {
            return new AppUser
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role"))
            };
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
