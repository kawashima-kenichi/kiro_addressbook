using AddressBook.API.Middleware;
using AddressBook.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace AddressBook.Tests.Middleware;

public class AuditLogMiddlewareTests
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogMiddleware> _logger;
    private readonly AuditLogMiddleware _middleware;
    private readonly RequestDelegate _next;

    public AuditLogMiddlewareTests()
    {
        _auditLogService = Substitute.For<IAuditLogService>();
        _logger = Substitute.For<ILogger<AuditLogMiddleware>>();
        _next = Substitute.For<RequestDelegate>();
        _middleware = new AuditLogMiddleware(_next, _logger);
    }

    [Fact]
    public async Task InvokeAsync_ContactsPostRequest_LogsAuditEntry()
    {
        // Arrange
        var context = CreateHttpContext("POST", "/api/contacts");
        var userId = Guid.NewGuid();
        context.User = CreateClaimsPrincipal(userId);

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _next.Received(1).Invoke(context);
        await _auditLogService.Received(1).LogAsync(
            userId,
            "Create",
            "Contact",
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task InvokeAsync_ContactsPutRequest_LogsAuditEntry()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var context = CreateHttpContext("PUT", $"/api/contacts/{contactId}");
        var userId = Guid.NewGuid();
        context.User = CreateClaimsPrincipal(userId);

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _next.Received(1).Invoke(context);
        await _auditLogService.Received(1).LogAsync(
            userId,
            "Update",
            "Contact",
            contactId,
            Arg.Any<string?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task InvokeAsync_ContactsDeleteRequest_LogsAuditEntry()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var context = CreateHttpContext("DELETE", $"/api/contacts/{contactId}");
        var userId = Guid.NewGuid();
        context.User = CreateClaimsPrincipal(userId);

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _next.Received(1).Invoke(context);
        await _auditLogService.Received(1).LogAsync(
            userId,
            "Delete",
            "Contact",
            contactId,
            Arg.Any<string?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task InvokeAsync_ContactsGetRequest_LogsAuditEntry()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/contacts");
        var userId = Guid.NewGuid();
        context.User = CreateClaimsPrincipal(userId);

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _next.Received(1).Invoke(context);
        await _auditLogService.Received(1).LogAsync(
            userId,
            "Read",
            "Contact",
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task InvokeAsync_AuthLoginRequest_LogsAuditEntry()
    {
        // Arrange
        var context = CreateHttpContext("POST", "/api/auth/login");

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _next.Received(1).Invoke(context);
        await _auditLogService.Received(1).LogAsync(
            Arg.Is<Guid?>(x => x == null),
            Arg.Is<string>(x => x == "Login"),
            Arg.Is<string>(x => x == "Session"),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task InvokeAsync_NonAuditableRequest_DoesNotLog()
    {
        // Arrange
        var context = CreateHttpContext("GET", "/api/health");

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _next.Received(1).Invoke(context);
        await _auditLogService.DidNotReceive().LogAsync(
            Arg.Any<Guid?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>());
    }

    [Fact]
    public async Task InvokeAsync_WithIpAddress_LogsIpAddress()
    {
        // Arrange
        var context = CreateHttpContext("POST", "/api/contacts");
        var userId = Guid.NewGuid();
        context.User = CreateClaimsPrincipal(userId);
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _auditLogService.Received(1).LogAsync(
            userId,
            "Create",
            "Contact",
            Arg.Any<Guid?>(),
            "192.168.1.100",
            Arg.Any<string?>());
    }

    [Fact]
    public async Task InvokeAsync_WithUserAgent_LogsUserAgent()
    {
        // Arrange
        var context = CreateHttpContext("POST", "/api/contacts");
        var userId = Guid.NewGuid();
        context.User = CreateClaimsPrincipal(userId);
        context.Request.Headers["User-Agent"] = "Mozilla/5.0";

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _auditLogService.Received(1).LogAsync(
            userId,
            "Create",
            "Contact",
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            "Mozilla/5.0");
    }

    [Fact]
    public async Task InvokeAsync_WithXForwardedFor_UsesForwardedIp()
    {
        // Arrange
        var context = CreateHttpContext("POST", "/api/contacts");
        var userId = Guid.NewGuid();
        context.User = CreateClaimsPrincipal(userId);
        context.Request.Headers["X-Forwarded-For"] = "203.0.113.1, 198.51.100.1";

        // Act
        await _middleware.InvokeAsync(context, _auditLogService);

        // Assert
        await _auditLogService.Received(1).LogAsync(
            userId,
            "Create",
            "Contact",
            Arg.Any<Guid?>(),
            "203.0.113.1",
            Arg.Any<string?>());
    }

    private static HttpContext CreateHttpContext(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        return context;
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(Guid userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }
}
