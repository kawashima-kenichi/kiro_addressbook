using AddressBook.Domain.Entities;
using AddressBook.Domain.Interfaces;
using AddressBook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AddressBook.Infrastructure.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly ApplicationDbContext _context;

    public ContactRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Contact?> GetByIdAsync(Guid id)
    {
        return await _context.Contacts.FindAsync(id);
    }

    public async Task<Contact?> FindByNameAsync(Guid userId, string name)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());
    }

    public async Task<(IEnumerable<Contact> Contacts, int TotalCount)> GetByUserIdAsync(
        Guid userId, int page, int pageSize, string? sortBy = null)
    {
        var query = _context.Contacts.Where(c => c.UserId == userId);

        // Default sort: case-insensitive alphabetical by name (Requirement 3.1)
        query = sortBy?.ToLower() switch
        {
            "name_desc" => query.OrderByDescending(c => c.Name.ToLower()),
            "created_at" => query.OrderBy(c => c.CreatedAt),
            "created_at_desc" => query.OrderByDescending(c => c.CreatedAt),
            "updated_at" => query.OrderBy(c => c.UpdatedAt),
            "updated_at_desc" => query.OrderByDescending(c => c.UpdatedAt),
            _ => query.OrderBy(c => c.Name.ToLower()) // Default: alphabetical, case-insensitive
        };

        var totalCount = await _context.Contacts.CountAsync(c => c.UserId == userId);

        // Pagination: 50 per page (Requirement 3.3)
        var contacts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (contacts, totalCount);
    }

    public async Task<Contact> CreateAsync(Contact contact)
    {
        contact.Id = Guid.NewGuid();
        contact.CreatedAt = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
        contact.UpdatedAt = DateTime.UtcNow;

        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync();

        return contact;
    }

    public async Task DeleteAsync(Guid id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        if (contact != null)
        {
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
        }
    }
}
