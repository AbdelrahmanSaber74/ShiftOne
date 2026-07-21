using System.Diagnostics;
using Microsoft.Extensions.Options;
using Serilog.Context;
using ShiftOne.Api.Constants;
using ShiftOne.Api.Options;

namespace ShiftOne.Api.Middleware
{
    public sealed class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly LoggingOptions _options;
        private readonly IHostEnvironment _environment;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            IOptions<LoggingOptions> options,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startedAtUtc = DateTimeOffset.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            var correlationId = ResolveCorrelationId(context, _options.CorrelationHeaderName);

            context.TraceIdentifier = correlationId;
            context.Items[ApiConstants.HttpContextItems.CorrelationId] = correlationId;
            context.Response.Headers[_options.CorrelationHeaderName] = correlationId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[_options.CorrelationHeaderName] = correlationId;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty(ApiConstants.LogProperties.ApplicationName, _options.ApplicationName))
            using (LogContext.PushProperty(ApiConstants.LogProperties.CorrelationId, correlationId))
            using (LogContext.PushProperty(ApiConstants.LogProperties.EnvironmentName, _environment.EnvironmentName))
            using (LogContext.PushProperty(ApiConstants.LogProperties.ClientIp, GetClientIp(context)))
            {
                try
                {
                    await _next(context);
                }
                finally
                {
                    stopwatch.Stop();
                    var statusCode = context.Response.StatusCode;
                    var level = statusCode >= StatusCodes.Status500InternalServerError
                        ? LogLevel.Error
                        : statusCode >= StatusCodes.Status400BadRequest
                            ? LogLevel.Warning
                            : LogLevel.Information;

                    _logger.Log(
                        level,
                        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ExecutionTimeMs} ms with CorrelationId {CorrelationId} from {ClientIp} at {TimestampUtc}",
                        context.Request.Method,
                        context.Request.Path.Value ?? "/",
                        statusCode,
                        stopwatch.ElapsedMilliseconds,
                        correlationId,
                        GetClientIp(context),
                        startedAtUtc);
                }
            }
        }

        private static string ResolveCorrelationId(HttpContext context, string headerName)
        {
            var incomingCorrelationId = context.Request.Headers[headerName].FirstOrDefault();
            return string.IsNullOrWhiteSpace(incomingCorrelationId)
                ? Guid.NewGuid().ToString("N")
                : incomingCorrelationId.Trim();
        }

        private static string GetClientIp(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}

