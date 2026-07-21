using System.Text.Json;
using ShiftOne.Api.Constants;
using ShiftOne.Shared.Responses;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ShiftOne.Api.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var correlationId = GetCorrelationId(context);
                _logger.LogError(
                    ex,
                    "Unhandled exception while processing {RequestMethod} {RequestPath}. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path.Value ?? "/",
                    correlationId);

                var localizer = context.RequestServices.GetRequiredService<ILocalizationService>();
                await WriteErrorResponseAsync(context, localizer);
            }
        }

        private static string GetCorrelationId(HttpContext context)
        {
            return context.Items.TryGetValue(ApiConstants.HttpContextItems.CorrelationId, out var value) && value is string correlationId
                ? correlationId
                : context.TraceIdentifier;
        }

        private static async Task WriteErrorResponseAsync(HttpContext context, ILocalizationService localizer)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            var message = localizer.GetString("Messages.InternalServerError");
            var response = GeneralResponse.InternalError(message);
            context.Response.Clear();
            context.Response.StatusCode = response.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
