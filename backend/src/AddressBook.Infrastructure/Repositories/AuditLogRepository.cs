using AddressBook.Domain.Entities;
using AddressBook.Domain.Interfaces;
using AddressBook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AddressBook.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        auditLog.Id = Guid.NewGuid();
        auditLog.CreatedAt = DateTime.UtcNow;

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return auditLog;
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int page, int pageSize)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
