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

            sql = _dialect.FormatSql(sql);

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, _dialect.FormatSql("@PageSize"), pageSize);

            if (cursor.HasValue)
            {
                AddParameter(command, _dialect.FormatSql("@Cursor"), cursor.Value);
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
            var sql = _dialect.FormatSql("SELECT Id, Title, Description, IsCompleted, CreatedDate FROM Tasks WHERE Id = @Id");

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, _dialect.FormatSql("@Id"), id);

            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTaskItem(reader) : null;
        }

        public async Task<TaskItem> AddAsync(TaskItem taskItem)
        {
            var sql = _dialect.FormatSql(_dialect.InsertReturningId(
                "Tasks",
                "Title, Description, IsCompleted, CreatedDate",
                "@Title, @Description, @IsCompleted, @CreatedDate"));

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, _dialect.FormatSql("@Title"), taskItem.Title);
            AddParameter(command, _dialect.FormatSql("@Description"), (object?)taskItem.Description ?? DBNull.Value);
            AddParameter(command, _dialect.FormatSql("@IsCompleted"), taskItem.IsCompleted);
            AddParameter(command, _dialect.FormatSql("@CreatedDate"), taskItem.CreatedDate);

            if (_dialect.RequiresOutputParameterForInsert)
            {
                var outParam = command.CreateParameter();
                outParam.ParameterName = "NewId";
                outParam.DbType = DbType.Int32;
                outParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(outParam);
                await command.ExecuteNonQueryAsync();
                taskItem.Id = Convert.ToInt32(outParam.Value);
            }
            else
            {
                taskItem.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            }

            return taskItem;
        }

        public async Task<bool> UpdateAsync(TaskItem taskItem)
        {
            var sql = _dialect.FormatSql(@"
                UPDATE Tasks
                SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted
                WHERE Id = @Id");

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, _dialect.FormatSql("@Id"), taskItem.Id);
            AddParameter(command, _dialect.FormatSql("@Title"), taskItem.Title);
            AddParameter(command, _dialect.FormatSql("@Description"), (object?)taskItem.Description ?? DBNull.Value);
            AddParameter(command, _dialect.FormatSql("@IsCompleted"), taskItem.IsCompleted);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sql = _dialect.FormatSql("DELETE FROM Tasks WHERE Id = @Id");

            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, _dialect.FormatSql("@Id"), id);

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
