using System.Net;
using System.Text.Json;
using AddressBook.API.Middleware;
using AddressBook.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AddressBook.Tests.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _mockLogger;
    private readonly GlobalExceptionHandlerMiddleware _middleware;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        _mockLogger = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
        
        // 正常なリクエストデリゲート
        RequestDelegate next = (HttpContext context) => Task.CompletedTask;
        _middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate throwingNext = (HttpContext ctx) => throw new Exception("Test exception");
        var middleware = new GlobalExceptionHandlerMiddleware(throwingNext, _mockLogger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ReturnsErrorResponseWithErrorId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate throwingNext = (HttpContext ctx) => throw new Exception("Test exception");
        var middleware = new GlobalExceptionHandlerMiddleware(throwingNext, _mockLogger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Equal("システムエラーが発生しました。もう一度お試しください。", errorResponse.Message);
        Assert.NotNull(errorResponse.ErrorId);
        Assert.StartsWith("err_", errorResponse.ErrorId);
        Assert.NotEqual(default(DateTime), errorResponse.Timestamp);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_LogsErrorWithDetails()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        
        RequestDelegate throwingNext = (HttpContext ctx) => throw new Exception("Test exception");
        var middleware = new GlobalExceptionHandlerMiddleware(throwingNext, _mockLogger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        // NSubstituteでログ呼び出しを検証
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        
        var middleware = new GlobalExceptionHandlerMiddleware(next, _mockLogger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ErrorIdFollowsCorrectFormat()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate throwingNext = (HttpContext ctx) => throw new Exception("Test exception");
        var middleware = new GlobalExceptionHandlerMiddleware(throwingNext, _mockLogger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(errorResponse);
        Assert.Matches(@"^err_\d{8}_[a-f0-9]{32}$", errorResponse.ErrorId);
    }
}
