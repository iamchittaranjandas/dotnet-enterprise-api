using DotnetEnterpriseApi.Api.Middleware;

namespace DotnetEnterpriseApi.Api.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseSwaggerDocumentation(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            return app;
        }

        public static WebApplication UseCustomMiddlewares(this WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRateLimiter();
            app.UseOutputCache();

            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static WebApplication UseHealthCheckEndpoint(this WebApplication app)
        {
            app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalMilliseconds + "ms"
                        }),
                        totalDuration = report.TotalDuration.TotalMilliseconds + "ms"
                    };
                    await context.Response.WriteAsJsonAsync(result);
                }
            });

            return app;
        }
    }
}
