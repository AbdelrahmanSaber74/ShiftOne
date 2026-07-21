using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ShiftOne.Api.Constants;
using ShiftOne.Api.Middleware;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;

namespace ShiftOne.Tests.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ReturnsCleanInternalErrorAndLogsExceptionWithCorrelationId()
    {
        const string correlationId = "exception-correlation-id";
        var logger = new TestLogger<ExceptionHandlingMiddleware>();
        var context = new DefaultHttpContext();
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        var localizerMock = new Mock<ILocalizationService>();
        localizerMock.Setup(l => l.GetString("Messages.InternalServerError"))
            .Returns("An unexpected error occurred.");
        localizerMock.Setup(l => l.GetString("Messages.InternalServerError", It.IsAny<System.Collections.Generic.Dictionary<string, string>>()))
            .Returns("An unexpected error occurred.");
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ShiftOne.Core.Interfaces.Infrastructure.Providers.ILocalizationService)))
            .Returns(localizerMock.Object);
        context.RequestServices = serviceProviderMock.Object;

        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/users/user/profile";
        context.Response.Body = new MemoryStream();
        context.Items[ApiConstants.HttpContextItems.CorrelationId] = correlationId;

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("sensitive failure details"),
            logger);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var entry = Assert.Single(logger.Entries);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Contains("An unexpected error occurred.", body);
        Assert.DoesNotContain("sensitive failure details", body, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.IsType<InvalidOperationException>(entry.Exception);
        Assert.Contains(correlationId, entry.Message);
        Assert.Contains("/api/users/user/profile", entry.Message);
    }
}
