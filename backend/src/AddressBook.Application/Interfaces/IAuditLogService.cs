namespace AddressBook.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(Guid? userId, string action, string resourceType, Guid? resourceId, string? ipAddress, string? userAgent);
}
