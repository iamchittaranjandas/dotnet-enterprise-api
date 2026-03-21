using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Infrastructure.Data;
using DotnetEnterpriseApi.Infrastructure.Data.Dialects;
using DotnetEnterpriseApi.Infrastructure.Persistence;
using DotnetEnterpriseApi.Infrastructure.Repositories.Dapper;
using DotnetEnterpriseApi.Infrastructure.Repositories.Ado;
using DotnetEnterpriseApi.Infrastructure.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetEnterpriseApi.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var dataProvider = configuration["DataProvider"] ?? "EntityFramework";

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

            services.AddDbContext<AppDbContext>(options =>
            {
                switch (databaseProvider.ToLowerInvariant())
                {
                    case "postgresql":
                        options.UseNpgsql(connectionString,
                            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                        break;
                    case "mysql":
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                        break;
                    case "oracle":
                        options.UseOracle(connectionString,
                            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                        break;
                    default:
                        options.UseSqlServer(connectionString,
                            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                        break;
                }
            });

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
