using System.Data;
using System.Data.Common;
using DotnetEnterpriseApi.Application.Common.Interfaces;

namespace DotnetEnterpriseApi.Infrastructure.Persistence
{
    public class DapperUnitOfWork : IUnitOfWork
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;

        public DapperUnitOfWork(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dapper/ADO.NET commits happen immediately per query — no change tracking
            return Task.FromResult(0);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _connection = _connectionFactory.CreateConnection();

            if (_connection is DbConnection dbConnection)
            {
                await dbConnection.OpenAsync(cancellationToken);
            }
            else
            {
                _connection.Open();
            }

            _transaction = _connection.BeginTransaction();
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
                _connection?.Dispose();
                _connection = null;
            }

            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
            _connection?.Dispose();
            _connection = null;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}
