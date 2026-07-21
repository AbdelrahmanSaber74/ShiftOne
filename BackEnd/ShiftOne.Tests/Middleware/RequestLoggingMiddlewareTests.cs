using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShiftOne.Api.Constants;
using ShiftOne.Api.Middleware;
using ShiftOne.Api.Options;
using System.Net;
using System.Text;

namespace ShiftOne.Tests.Middleware;

public sealed class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_GeneratesCorrelationId_WhenHeaderIsMissing()
    {
        var logger = new TestLogger<RequestLoggingMiddleware>();
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(logger, async httpContext =>
        {
            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            await httpContext.Response.StartAsync();
        });

        await middleware.InvokeAsync(context);

        var correlationId = Assert.IsType<string>(context.Items[ApiConstants.HttpContextItems.CorrelationId]);
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.Equal(correlationId, context.TraceIdentifier);
        Assert.Equal(correlationId, context.Response.Headers[ApiConstants.Headers.CorrelationId].ToString());
    }

    [Fact]
    public async Task InvokeAsync_PreservesIncomingCorrelationId()
    {
        const string correlationId = "existing-correlation-id";
        var logger = new TestLogger<RequestLoggingMiddleware>();
        var context = CreateHttpContext();
        context.Request.Headers[ApiConstants.Headers.CorrelationId] = correlationId;
        var middleware = CreateMiddleware(logger, async httpContext =>
        {
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            await httpContext.Response.WriteAsync("ok");
        });

        await middleware.InvokeAsync(context);

        Assert.Equal(correlationId, context.Items[ApiConstants.HttpContextItems.CorrelationId]);
        Assert.Equal(correlationId, context.TraceIdentifier);
        Assert.Equal(correlationId, context.Response.Headers[ApiConstants.Headers.CorrelationId].ToString());
    }

    [Fact]
    public async Task InvokeAsync_LogsSafeRequestMetadataWithoutSensitiveValues()
    {
        var logger = new TestLogger<RequestLoggingMiddleware>();
        var context = CreateHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/users/user/login";
        context.Request.Headers.Authorization = "Bearer secret-token";
        context.Request.Headers.Cookie = "session=secret-cookie";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"password\":\"secret-password\"}"));
        var middleware = CreateMiddleware(logger, async httpContext =>
        {
            httpContext.Response.StatusCode = StatusCodes.Status201Created;
            await httpContext.Response.WriteAsync("created");
        });

        await middleware.InvokeAsync(context);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Contains("POST", entry.Message);
        Assert.Contains("/api/users/user/login", entry.Message);
        Assert.DoesNotContain("Authorization", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Cookie", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-token", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-password", entry.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static RequestLoggingMiddleware CreateMiddleware(
        TestLogger<RequestLoggingMiddleware> logger,
        RequestDelegate next)
    {
        return new RequestLoggingMiddleware(
            next,
            logger,
            Options.Create(new LoggingOptions()),
            new TestHostEnvironment());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        return context;
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = ApiConstants.Application.Name;
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
