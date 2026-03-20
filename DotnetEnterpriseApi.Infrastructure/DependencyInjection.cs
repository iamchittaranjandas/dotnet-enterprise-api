using DotnetEnterpriseApi.Application.Common.Interfaces;
using DotnetEnterpriseApi.Application.Interfaces;
using DotnetEnterpriseApi.Infrastructure.Data;
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
                services.AddDapperInfrastructure();
            }
            else if (dataProvider.Equals("Ado", StringComparison.OrdinalIgnoreCase))
            {
                services.AddAdoInfrastructure();
            }
            else
            {
                services.AddEntityFrameworkInfrastructure(configuration);
            }

            return services;
        }

        private static void AddEntityFrameworkInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITaskRepository, EfTaskRepository>();
            services.AddScoped<IUserRepository, EfUserRepository>();
        }

        private static void AddDapperInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
            services.AddScoped<ITaskRepository, DapperTaskRepository>();
            services.AddScoped<IUserRepository, DapperUserRepository>();
        }

        private static void AddAdoInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
            services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
            services.AddScoped<ITaskRepository, AdoTaskRepository>();
            services.AddScoped<IUserRepository, AdoUserRepository>();
        }
    }
}
