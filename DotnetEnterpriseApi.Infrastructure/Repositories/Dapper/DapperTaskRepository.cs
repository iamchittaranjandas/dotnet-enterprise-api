using Dapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Dapper
{
    public class DapperTaskRepository : ITaskRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly ISqlDialect _dialect;

        public DapperTaskRepository(ISqlConnectionFactory connectionFactory, ISqlDialect dialect)
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

            using var connection = _connectionFactory.CreateConnection();
            var result = cursor.HasValue
                ? await connection.QueryAsync<TaskItem>(sql, new { Cursor = cursor.Value, PageSize = pageSize })
                : await connection.QueryAsync<TaskItem>(sql, new { PageSize = pageSize });

            return result.ToList();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            const string sql = "SELECT Id, Title, Description, IsCompleted, CreatedDate FROM Tasks WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<TaskItem>(sql, new { Id = id });
        }

        public async Task<TaskItem> AddAsync(TaskItem taskItem)
        {
            var sql = _dialect.InsertReturningId(
                "Tasks",
                "Title, Description, IsCompleted, CreatedDate",
                "@Title, @Description, @IsCompleted, @CreatedDate");

            using var connection = _connectionFactory.CreateConnection();
            var id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                taskItem.Title,
                taskItem.Description,
                taskItem.IsCompleted,
                taskItem.CreatedDate
            });

            taskItem.Id = id;
            return taskItem;
        }

        public async Task<bool> UpdateAsync(TaskItem taskItem)
        {
            const string sql = @"
                UPDATE Tasks
                SET Title = @Title, Description = @Description, IsCompleted = @IsCompleted
                WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new
            {
                taskItem.Id,
                taskItem.Title,
                taskItem.Description,
                taskItem.IsCompleted
            });

            return affected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Tasks WHERE Id = @Id";

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });

            return affected > 0;
        }
    }
}
