using System.Data;
using DotnetEnterpriseApi.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DotnetEnterpriseApi.Infrastructure.Data
{
    public class DatabaseConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private readonly string _databaseProvider;

        public DatabaseConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _databaseProvider = configuration["DatabaseProvider"] ?? "SqlServer";
        }

        public IDbConnection CreateConnection()
        {
            return _databaseProvider.ToLowerInvariant() switch
            {
                "postgresql" => new NpgsqlConnection(_connectionString),
                "sqlite" => new SqliteConnection(_connectionString),
                _ => new SqlConnection(_connectionString),
            };
        }
    }
}
