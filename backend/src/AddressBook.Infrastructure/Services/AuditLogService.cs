using AddressBook.Application.Interfaces;
using AddressBook.Domain.Entities;
using AddressBook.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AddressBook.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IAuditLogRepository auditLogRepository, ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task LogAsync(Guid? userId, string action, string resourceType, Guid? resourceId, string? ipAddress, string? userAgent)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                ResourceType = resourceType,
                ResourceId = resourceId,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _auditLogRepository.CreateAsync(auditLog);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - audit logging should not break the main operation
            _logger.LogError(ex, "監査ログの記録中にエラーが発生しました: UserId={UserId}, Action={Action}, ResourceType={ResourceType}", 
                userId, action, resourceType);
        }
    }
}
