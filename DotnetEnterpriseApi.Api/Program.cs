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
            builder.Services.AddApiVersioningConfiguration();
            builder.Services.AddHealthChecksConfiguration(builder.Configuration);
            builder.Services.AddRateLimiting();
            builder.Services.AddOutputCaching();
            builder.Services.AddOpenTelemetryConfiguration();

            var app = builder.Build();

            // ── Middleware Pipeline ──
            app.UseSwaggerDocumentation();
            app.UseCustomMiddlewares();
            app.MapControllers();
            app.UseHealthCheckEndpoint();

            app.Run();
        }
    }
}
