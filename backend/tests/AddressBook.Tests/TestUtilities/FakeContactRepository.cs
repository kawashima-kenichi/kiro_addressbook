using AddressBook.Domain.Entities;
using AddressBook.Domain.Interfaces;

namespace AddressBook.Tests.TestUtilities;

/// <summary>
/// In-memory fake implementation of IContactRepository for testing purposes
/// </summary>
public class FakeContactRepository : IContactRepository
{
    private readonly List<Contact> _contacts = new();

    public Task<Contact?> GetByIdAsync(Guid id)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(contact);
    }

    public Task<Contact?> FindByNameAsync(Guid userId, string name)
    {
        var contact = _contacts.FirstOrDefault(c => 
            c.UserId == userId && 
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(contact);
    }

    public Task<(IEnumerable<Contact> Contacts, int TotalCount)> GetByUserIdAsync(
        Guid userId, int page, int pageSize, string? sortBy = null)
    {
        var userContacts = _contacts.Where(c => c.UserId == userId).ToList();
        var totalCount = userContacts.Count;
        
        var sortedContacts = sortBy?.ToLower() switch
        {
            "name_desc" => userContacts.OrderByDescending(c => c.Name.ToLower()),
            _ => userContacts.OrderBy(c => c.Name.ToLower())
        };

        var pagedContacts = sortedContacts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<(IEnumerable<Contact>, int)>((pagedContacts, totalCount));
    }

    public Task<Contact> CreateAsync(Contact contact)
    {
        if (contact.Id == Guid.Empty)
        {
            contact.Id = Guid.NewGuid();
        }
        contact.CreatedAt = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;
        _contacts.Add(contact);
        return Task.FromResult(contact);
    }

    public Task<Contact> UpdateAsync(Contact contact)
    {
        var existing = _contacts.FirstOrDefault(c => c.Id == contact.Id);
        if (existing != null)
        {
            _contacts.Remove(existing);
            contact.UpdatedAt = DateTime.UtcNow;
            _contacts.Add(contact);
        }
        return Task.FromResult(contact);
    }

    public Task DeleteAsync(Guid id)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == id);
        if (contact != null)
        {
            _contacts.Remove(contact);
        }
        return Task.CompletedTask;
    }
}
