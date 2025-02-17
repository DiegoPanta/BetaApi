using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices
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

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Capture Client IP
            var clientIP = context.Connection.RemoteIpAddress?.ToString();

            // Capture Request Details
            var logRequest = new
            {
                Timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Level = "Information",
                Message = "Incoming request",
                Properties = new
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    ClientIP = clientIP
                }
            };

            // Log Request as JSON
            _logger.LogInformation("{@logRequest}", logRequest);

            await _next(context);

            stopwatch.Stop();

            // Capture Response Details
            var logResponse = new
            {
                Timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Level = "Information",
                Message = "Request completed",
                Properties = new
                {
                    StatusCode = context.Response.StatusCode,
                    Duration = stopwatch.ElapsedMilliseconds
                }
            };

            // Log Response as JSON
            _logger.LogInformation("{@logResponse}", logResponse);
        }
    }
}
