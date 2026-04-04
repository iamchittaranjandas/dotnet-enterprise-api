using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Infrastructure.Data;
using DotnetEnterpriseApi.Infrastructure.Data.Dialects;
using DotnetEnterpriseApi.Infrastructure.Persistence;
using DotnetEnterpriseApi.Infrastructure.Repositories.Dapper;
using DotnetEnterpriseApi.Infrastructure.Repositories.Ado;
using DotnetEnterpriseApi.Infrastructure.Repositories.EntityFramework;
using DotnetEnterpriseApi.Infrastructure.Repositories.VectorStore;
using DotnetEnterpriseApi.Infrastructure.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetEnterpriseApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProvider     = configuration["DataProvider"]    ?? "EntityFramework";
            var databaseProvider = configuration["DatabaseProvider"] ?? "SqlServer";
            var isPostgres       = dataProvider.Equals("EntityFramework", StringComparison.OrdinalIgnoreCase)
                                   && databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase);

            // RAG service: pgvector when PostgreSQL+EF, in-memory otherwise
            if (isPostgres)
                services.AddSingleton<ITaskRagService, PgVectorRagService>();
            else
                services.AddSingleton<ITaskRagService, TaskRagService>();

            services.AddHostedService<RagRehydrationService>();
            services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

            if (dataProvider.Equals("Dapper", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDapperInfrastructure(configuration);
            }
            else if (dataProvider.Equals("Ado", StringComparison.OrdinalIgnoreCase))
            {
                services.AddAdoInfrastructure(configuration);
            }
            else
            {
                services.AddEntityFrameworkInfrastructure(configuration);
            }

            return services;
        }

        private static void AddEntityFrameworkInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseProvider = configuration["DatabaseProvider"] ?? "SqlServer";
            var connectionString = configuration.GetConnectionString("DefaultConnection")!;

            // Single registration — AddDbContextFactory registers both the factory (singleton)
            // and IDbContextOptions. The scoped AppDbContext is resolved from the factory below.
            services.AddDbContextFactory<AppDbContext>(options =>
            {
                switch (databaseProvider.ToLowerInvariant())
                {
                    case "postgresql":
                        options.UseNpgsql(connectionString, b =>
                        {
                            b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                            b.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
                            b.UseVector();
                        });
                        break;
                    case "mysql":
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), b =>
                        {
                            b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                            b.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                        });
                        break;
                    case "oracle":
                        options.UseOracle(connectionString, b =>
                            b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                        break;
                    default:
                        options.UseSqlServer(connectionString, b =>
                        {
                            b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                            b.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                        });
                        break;
                }
            });

            // Resolve the scoped AppDbContext from the factory for the regular request pipeline
            services.AddScoped<AppDbContext>(provider =>
                provider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITaskRepository, EfTaskRepository>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
        }

        private static void AddDapperInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISqlConnectionFactory, DatabaseConnectionFactory>();
            services.AddSingleton<ISqlDialect>(ResolveSqlDialect(configuration));
            services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
            services.AddScoped<ITaskRepository, DapperTaskRepository>();
            services.AddScoped<IUserRepository, DapperUserRepository>();
            services.AddScoped<IRefreshTokenRepository, DapperRefreshTokenRepository>();
        }

        private static void AddAdoInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISqlConnectionFactory, DatabaseConnectionFactory>();
            services.AddSingleton<ISqlDialect>(ResolveSqlDialect(configuration));
            services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
            services.AddScoped<ITaskRepository, AdoTaskRepository>();
            services.AddScoped<IUserRepository, AdoUserRepository>();
            services.AddScoped<IRefreshTokenRepository, AdoRefreshTokenRepository>();
        }

        private static ISqlDialect ResolveSqlDialect(IConfiguration configuration)
        {
            var provider = configuration["DatabaseProvider"] ?? "SqlServer";
            return provider.ToLowerInvariant() switch
            {
                "postgresql" => new PostgreSqlDialect(),
                "mysql" => new MySqlDialect(),
                "oracle" => new OracleDialect(),
                _ => new SqlServerDialect(),
            };
        }
    }
}
