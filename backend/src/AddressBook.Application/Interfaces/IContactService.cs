using AddressBook.Application.DTOs;

namespace AddressBook.Application.Interfaces;

public interface IContactService
{
    Task<ContactDto> CreateContactAsync(Guid userId, CreateContactRequest request);
    Task<ContactDto?> GetContactByIdAsync(Guid userId, Guid contactId);
    Task<(IEnumerable<ContactDto> Contacts, int TotalCount)> GetContactsAsync(Guid userId, int page, int pageSize, string? sortBy = null);
    Task<ContactDto> UpdateContactAsync(Guid userId, Guid contactId, UpdateContactRequest request);
    Task DeleteContactAsync(Guid userId, Guid contactId);
}
