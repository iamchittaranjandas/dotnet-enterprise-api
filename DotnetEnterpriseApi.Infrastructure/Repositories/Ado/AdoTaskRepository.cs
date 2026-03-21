using System.Data;
using System.Data.Common;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Ado
{
    public class AdoTaskRepository : ITaskRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly ISqlDialect _dialect;

        public AdoTaskRepository(ISqlConnectionFactory connectionFactory, ISqlDialect dialect)
        {
            _connectionFactory = connectionFactory;
            _dialect = dialect;
        }

        public async Task<List<TaskItem>> GetAllAsync(int? cursor, int pageSize)
        {
            var sql = cursor.HasValue
                ? _dialect.PaginateQuery(
                    "Id, Title, Description, IsCompleted, CreatedDate",
                    "Tasks",
                    "Id < @Cursor",
                    "ORDER BY Id DESC")
                : _dialect.PaginateQuery(
                    "Id, Title, Description, IsCompleted, CreatedDate",
                    "Tasks",
                    null,
                    "ORDER BY Id DESC");

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@PageSize", pageSize);

            if (cursor.HasValue)
            {
                AddParameter(command, "@Cursor", cursor.Value);
            }

            using var reader = await command.ExecuteReaderAsync();
            var tasks = new List<TaskItem>();

            while (await reader.ReadAsync())
            {
                tasks.Add(MapTaskItem(reader));
            }

            return tasks;
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            const string sql = "SELECT Id, Title, Description, IsCompleted, CreatedDate FROM Tasks WHERE Id = @Id";

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTaskItem(reader) : null;
        }

        public async Task<TaskItem> AddAsync(TaskItem taskItem)
        {
            var sql = _dialect.InsertReturningId(
                "Tasks",
                "Title, Description, IsCompleted, CreatedDate",
                "@Title, @Description, @IsCompleted, @CreatedDate");

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Title", taskItem.Title);
            AddParameter(command, "@Description", (object?)taskItem.Description ?? DBNull.Value);
            AddParameter(command, "@IsCompleted", taskItem.IsCompleted);
            AddParameter(command, "@CreatedDate", taskItem.CreatedDate);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            taskItem.Id = id;
            return taskItem;
        }

        public async Task<bool> UpdateAsync(TaskItem taskItem)
        {
            const string sql = @"
                UPDATE Tasks
                SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted
                WHERE Id = @Id";

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Id", taskItem.Id);
            AddParameter(command, "@Title", taskItem.Title);
            AddParameter(command, "@Description", (object?)taskItem.Description ?? DBNull.Value);
            AddParameter(command, "@IsCompleted", taskItem.IsCompleted);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Tasks WHERE Id = @Id";

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@Id", id);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        private static TaskItem MapTaskItem(IDataReader reader)
        {
            return new TaskItem
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? string.Empty
                    : reader.GetString(reader.GetOrdinal("Description")),
                IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
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
