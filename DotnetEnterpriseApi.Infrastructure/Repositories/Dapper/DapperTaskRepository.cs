using System.Data;
using Dapper;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Domain.Entities;
using DotnetEnterpriseApi.Infrastructure.Repositories.Queries;

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
            var sql = TaskQueries.GetAllPaginated(_dialect, cursor.HasValue);

            using var connection = _connectionFactory.CreateConnection();
            var result = cursor.HasValue
                ? await connection.QueryAsync<TaskItem>(sql, new { Cursor = cursor.Value, PageSize = pageSize })
                : await connection.QueryAsync<TaskItem>(sql, new { PageSize = pageSize });

            return result.ToList();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            var sql = TaskQueries.GetById(_dialect);

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<TaskItem>(sql, new { Id = id });
        }

        public async Task<TaskItem> AddAsync(TaskItem taskItem)
        {
            var sql = TaskQueries.Insert(_dialect);

            using var connection = _connectionFactory.CreateConnection();

            if (_dialect.RequiresOutputParameterForInsert)
            {
                var parameters = new DynamicParameters();
                parameters.Add("Title", taskItem.Title);
                parameters.Add("Description", taskItem.Description);
                parameters.Add("IsCompleted", taskItem.IsCompleted);
                parameters.Add("CreatedDate", taskItem.CreatedDate);
                parameters.Add("NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await connection.ExecuteAsync(sql, parameters);
                taskItem.Id = parameters.Get<int>("NewId");
            }
            else
            {
                var result = await connection.ExecuteScalarAsync<object>(sql, new
                {
                    taskItem.Title,
                    taskItem.Description,
                    taskItem.IsCompleted,
                    taskItem.CreatedDate
                });
                taskItem.Id = Convert.ToInt32(result);
            }

            return taskItem;
        }

        public async Task<bool> UpdateAsync(TaskItem taskItem)
        {
            var sql = TaskQueries.Update(_dialect);

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
            var sql = TaskQueries.Delete(_dialect);

            using var connection = _connectionFactory.CreateConnection();
            var affected = await connection.ExecuteAsync(sql, new { Id = id });

            return affected > 0;
        }
    }
}
