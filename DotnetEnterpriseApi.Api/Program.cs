using DotnetEnterpriseApi.Api.Extensions;
using DotnetEnterpriseApi.Application;
using DotnetEnterpriseApi.Infrastructure;

namespace DotnetEnterpriseApi.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Core Services ──
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // ── Application & Infrastructure ──
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            // ── API Features ──
            builder.Services.AddSwaggerDocumentation();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddCorsPolicy(builder.Configuration);
            builder.Services.AddResponseCompressionConfiguration();
            builder.Services.AddApiVersioningConfiguration();
            builder.Services.AddHealthChecksConfiguration(builder.Configuration);
            builder.Services.AddRateLimiting();
            builder.Services.AddRedisCaching(builder.Configuration);
            builder.Services.AddOutputCaching(builder.Configuration);
            builder.Services.AddOpenTelemetryConfiguration();
            builder.Services.AddAgentServices(builder.Configuration);

            var app = builder.Build();

            // ── Auto-create database schema for non-SQL Server providers ──
            var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
            if (!databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<DotnetEnterpriseApi.Infrastructure.Data.AppDbContext>();
                dbContext?.Database.EnsureCreated();
            }

            // ── Middleware Pipeline ──
            app.UseSwaggerDocumentation();
            app.UseCustomMiddlewares();
            app.MapControllers();
            app.UseHealthCheckEndpoint();

            app.Run();
        }
    }
}
