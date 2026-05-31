using AddressBook.Domain.Entities;

namespace AddressBook.Domain.Interfaces;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id);
    Task<Contact?> FindByNameAsync(Guid userId, string name);
    Task<(IEnumerable<Contact> Contacts, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize, string? sortBy = null);
    Task<Contact> CreateAsync(Contact contact);
    Task<Contact> UpdateAsync(Contact contact);
    Task DeleteAsync(Guid id);
}
