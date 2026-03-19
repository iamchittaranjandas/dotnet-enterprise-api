namespace DotnetEnterpriseApi.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation(
                "Incoming Request: {Method} {Path} at {Time}",
                context.Request.Method,
                context.Request.Path,
                DateTime.UtcNow);

            await _next(context);

            _logger.LogInformation(
                "Outgoing Response: {StatusCode} for {Method} {Path} at {Time}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path,
                DateTime.UtcNow);
        }
    }
}
