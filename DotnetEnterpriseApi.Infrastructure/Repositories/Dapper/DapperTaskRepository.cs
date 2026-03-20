using Dapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;

namespace DotnetEnterpriseApi.Infrastructure.Repositories.Dapper
{
    public class DapperTaskRepository : ITaskRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public DapperTaskRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<TaskItem>> GetAllAsync(int pageNumber, int pageSize)
        {
            const string sql = @"
                SELECT Id, Title, Description, IsCompleted, CreatedDate
                FROM Tasks
                ORDER BY CreatedDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryAsync<TaskItem>(sql, new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });

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
            const string sql = @"
                INSERT INTO Tasks (Title, Description, IsCompleted, CreatedDate)
                OUTPUT INSERTED.Id
                VALUES (@Title, @Description, @IsCompleted, @CreatedDate)";

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
