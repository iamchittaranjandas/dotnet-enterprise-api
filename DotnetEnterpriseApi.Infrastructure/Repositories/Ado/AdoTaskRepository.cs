using System.Data;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using Microsoft.Data.SqlClient;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Ado
{
    public class AdoTaskRepository : ITaskRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public AdoTaskRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<TaskItem>> GetAllAsync(int? cursor, int pageSize)
        {
            var sql = cursor.HasValue
                ? @"SELECT TOP (@PageSize) Id, Title, Description, IsCompleted, CreatedDate
                    FROM Tasks
                    WHERE Id < @Cursor
                    ORDER BY Id DESC"
                : @"SELECT TOP (@PageSize) Id, Title, Description, IsCompleted, CreatedDate
                    FROM Tasks
                    ORDER BY Id DESC";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PageSize", pageSize);

            if (cursor.HasValue)
            {
                command.Parameters.AddWithValue("@Cursor", cursor.Value);
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

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTaskItem(reader) : null;
        }

        public async Task<TaskItem> AddAsync(TaskItem taskItem)
        {
            const string sql = @"
                INSERT INTO Tasks (Title, Description, IsCompleted, CreatedDate)
                OUTPUT INSERTED.Id
                VALUES (@Title, @Description, @IsCompleted, @CreatedDate)";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Title", taskItem.Title);
            command.Parameters.AddWithValue("@Description", (object?)taskItem.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsCompleted", taskItem.IsCompleted);
            command.Parameters.AddWithValue("@CreatedDate", taskItem.CreatedDate);

            var id = (int)(await command.ExecuteScalarAsync())!;
            taskItem.Id = id;
            return taskItem;
        }

        public async Task<bool> UpdateAsync(TaskItem taskItem)
        {
            const string sql = @"
                UPDATE Tasks
                SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted
                WHERE Id = @Id";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", taskItem.Id);
            command.Parameters.AddWithValue("@Title", taskItem.Title);
            command.Parameters.AddWithValue("@Description", (object?)taskItem.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsCompleted", taskItem.IsCompleted);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Tasks WHERE Id = @Id";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

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
    }
}
