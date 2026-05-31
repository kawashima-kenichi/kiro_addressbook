using AddressBook.Application.DTOs;
using AddressBook.Application.Interfaces;
using AddressBook.Domain.Entities;
using AddressBook.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AddressBook.Infrastructure.Services;

public class ContactService : IContactService
{
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ContactService> _logger;

    public ContactService(
        IContactRepository contactRepository,
        IMapper mapper,
        ILogger<ContactService> logger)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ContactDto> CreateContactAsync(Guid userId, CreateContactRequest request)
    {
        // Check for duplicate name (case-insensitive) - Requirement 2.4, 8.2
        var existingContact = await _contactRepository.FindByNameAsync(userId, request.Name);
        if (existingContact != null)
        {
            _logger.LogWarning("Duplicate contact name attempt: {Name} for user {UserId}", request.Name, userId);
            throw new DuplicateContactNameException("この名前の連絡先は既に存在します");
        }

        // Map DTO to Entity
        var contact = _mapper.Map<Contact>(request);
        contact.UserId = userId;

        // Create contact - Requirement 2.1
        var createdContact = await _contactRepository.CreateAsync(contact);

        _logger.LogInformation("Contact created: {ContactId} for user {UserId}", createdContact.Id, userId);

        // Map Entity to DTO
        return _mapper.Map<ContactDto>(createdContact);
    }

    public async Task<ContactDto?> GetContactByIdAsync(Guid userId, Guid contactId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        
        // Verify ownership - Requirement 9.1
        if (contact == null || contact.UserId != userId)
        {
            if (contact != null)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} tried to access contact {ContactId} owned by {OwnerId}",
                    userId, contactId, contact.UserId);
            }
            return null;
        }

        return _mapper.Map<ContactDto>(contact);
    }

    public async Task<(IEnumerable<ContactDto> Contacts, int TotalCount)> GetContactsAsync(
        Guid userId, int page, int pageSize, string? sortBy = null)
    {
        // Get contacts with pagination and sorting - Requirement 3.1
        var (contacts, totalCount) = await _contactRepository.GetByUserIdAsync(userId, page, pageSize, sortBy);
        var contactDtos = _mapper.Map<IEnumerable<ContactDto>>(contacts);
        
        _logger.LogInformation("Retrieved {Count} contacts for user {UserId} (page {Page})", 
            contactDtos.Count(), userId, page);

        return (contactDtos, totalCount);
    }

    public async Task<ContactDto> UpdateContactAsync(Guid userId, Guid contactId, UpdateContactRequest request)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        
        // Verify ownership - Requirement 9.1
        if (contact == null || contact.UserId != userId)
        {
            if (contact != null)
            {
                _logger.LogWarning("Unauthorized update attempt: User {UserId} tried to update contact {ContactId} owned by {OwnerId}",
                    userId, contactId, contact.UserId);
            }
            throw new ContactNotFoundException("連絡先が見つかりません");
        }

        // Check for duplicate name if name is being changed (case-insensitive) - Requirement 2.4, 8.2
        if (!contact.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existingContact = await _contactRepository.FindByNameAsync(userId, request.Name);
            if (existingContact != null)
            {
                _logger.LogWarning("Duplicate contact name on update: {Name} for user {UserId}", request.Name, userId);
                throw new DuplicateContactNameException("この名前の連絡先は既に存在します");
            }
        }

        // Update contact properties - Requirement 4.2
        contact.Name = request.Name;
        contact.Address = request.Address;
        contact.PhoneNumber = request.PhoneNumber;

        var updatedContact = await _contactRepository.UpdateAsync(contact);

        _logger.LogInformation("Contact updated: {ContactId} for user {UserId}", contactId, userId);

        return _mapper.Map<ContactDto>(updatedContact);
    }

    public async Task DeleteContactAsync(Guid userId, Guid contactId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        
        // Verify ownership - Requirement 9.1, 5.2
        if (contact == null || contact.UserId != userId)
        {
            if (contact != null)
            {
                _logger.LogWarning("Unauthorized delete attempt: User {UserId} tried to delete contact {ContactId} owned by {OwnerId}",
                    userId, contactId, contact.UserId);
            }
            throw new ContactNotFoundException("連絡先が見つかりません");
        }

        await _contactRepository.DeleteAsync(contactId);

        _logger.LogInformation("Contact deleted: {ContactId} for user {UserId}", contactId, userId);
    }
}

// Custom exception types for contact operations
public class DuplicateContactNameException : Exception
{
    public DuplicateContactNameException(string message) : base(message) { }
}

public class ContactNotFoundException : Exception
{
    public ContactNotFoundException(string message) : base(message) { }
}
