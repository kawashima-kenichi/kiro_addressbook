using AddressBook.Application.Interfaces;
using AddressBook.Domain.Entities;
using AddressBook.Domain.Interfaces;
using AddressBook.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AddressBook.Tests.Services;

public class AuditLogServiceTests
{
    private readonly IAuditLogService _auditLogService;
    private readonly FakeAuditLogRepository _fakeRepository;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogServiceTests()
    {
        _fakeRepository = new FakeAuditLogRepository();
        _logger = Substitute.For<ILogger<AuditLogService>>();
        _auditLogService = new AuditLogService(_fakeRepository, _logger);
    }

    [Fact]
    public async Task LogAsync_ValidData_CreatesAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var action = "Create";
        var resourceType = "Contact";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act
        await _auditLogService.LogAsync(userId, action, resourceType, resourceId, ipAddress, userAgent);

        // Assert
        var logs = await _fakeRepository.GetByUserIdAsync(userId, 1, 10);
        var log = logs.FirstOrDefault();
        
        Assert.NotNull(log);
        Assert.Equal(userId, log.UserId);
        Assert.Equal(action, log.Action);
        Assert.Equal(resourceType, log.ResourceType);
        Assert.Equal(resourceId, log.ResourceId);
        Assert.Equal(ipAddress, log.IpAddress);
        Assert.Equal(userAgent, log.UserAgent);
    }

    [Fact]
    public async Task LogAsync_NullUserId_CreatesAuditLogWithNullUserId()
    {
        // Arrange
        Guid? userId = null;
        var action = "Login";
        var resourceType = "Session";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act
        await _auditLogService.LogAsync(userId, action, resourceType, null, ipAddress, userAgent);

        // Assert
        var allLogs = _fakeRepository.GetAllLogs();
        var log = allLogs.FirstOrDefault(l => l.Action == action);
        
        Assert.NotNull(log);
        Assert.Null(log.UserId);
        Assert.Equal(action, log.Action);
        Assert.Equal(resourceType, log.ResourceType);
    }

    [Fact]
    public async Task LogAsync_NullResourceId_CreatesAuditLogWithNullResourceId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var action = "Read";
        var resourceType = "Contact";
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act
        await _auditLogService.LogAsync(userId, action, resourceType, null, ipAddress, userAgent);

        // Assert
        var logs = await _fakeRepository.GetByUserIdAsync(userId, 1, 10);
        var log = logs.FirstOrDefault();
        
        Assert.NotNull(log);
        Assert.Null(log.ResourceId);
    }

    [Fact]
    public async Task LogAsync_NullIpAddressAndUserAgent_CreatesAuditLog()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var action = "Update";
        var resourceType = "Contact";
        var resourceId = Guid.NewGuid();

        // Act
        await _auditLogService.LogAsync(userId, action, resourceType, resourceId, null, null);

        // Assert
        var logs = await _fakeRepository.GetByUserIdAsync(userId, 1, 10);
        var log = logs.FirstOrDefault();
        
        Assert.NotNull(log);
        Assert.Null(log.IpAddress);
        Assert.Null(log.UserAgent);
    }

    [Fact]
    public async Task LogAsync_MultipleActions_CreatesMultipleAuditLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var actions = new[] { "Create", "Read", "Update", "Delete" };

        // Act
        foreach (var action in actions)
        {
            await _auditLogService.LogAsync(userId, action, "Contact", Guid.NewGuid(), "192.168.1.1", "Mozilla/5.0");
        }

        // Assert
        var logs = await _fakeRepository.GetByUserIdAsync(userId, 1, 10);
        Assert.Equal(4, logs.Count());
    }

    [Fact]
    public async Task LogAsync_RepositoryThrowsException_DoesNotThrow()
    {
        // Arrange
        var failingRepository = Substitute.For<IAuditLogRepository>();
        failingRepository.CreateAsync(Arg.Any<AuditLog>())
            .Returns<AuditLog>(_ => throw new Exception("Database error"));
        
        var logger = Substitute.For<ILogger<AuditLogService>>();
        var service = new AuditLogService(failingRepository, logger);
        var userId = Guid.NewGuid();

        // Act & Assert - should not throw
        await service.LogAsync(userId, "Create", "Contact", Guid.NewGuid(), "192.168.1.1", "Mozilla/5.0");
        
        // Verify that the service handled the exception gracefully (no exception thrown)
        // The logger should have been called, but we don't need to verify the exact parameters
        Assert.True(true); // Test passes if no exception was thrown
    }
}

// Fake repository for testing
public class FakeAuditLogRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _auditLogs = new();

    public Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        auditLog.Id = Guid.NewGuid();
        auditLog.CreatedAt = DateTime.UtcNow;
        _auditLogs.Add(auditLog);
        return Task.FromResult(auditLog);
    }

    public Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int page, int pageSize)
    {
        var logs = _auditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        return Task.FromResult<IEnumerable<AuditLog>>(logs);
    }

    public List<AuditLog> GetAllLogs()
    {
        return _auditLogs;
    }
}
