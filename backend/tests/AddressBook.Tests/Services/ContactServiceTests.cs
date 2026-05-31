using AddressBook.Application.DTOs;
using AddressBook.Application.Interfaces;
using AddressBook.Domain.Entities;
using AddressBook.Domain.Interfaces;
using AddressBook.Infrastructure.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AddressBook.Tests.Services;

public class ContactServiceTests
{
    private readonly IContactService _contactService;
    private readonly FakeContactRepository _fakeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ContactService> _logger;

    public ContactServiceTests()
    {
        _fakeRepository = new FakeContactRepository();
        
        // Create a simple mock mapper
        _mapper = Substitute.For<IMapper>();
        
        // Setup mapper to convert Contact to ContactDto
        _mapper.Map<ContactDto>(Arg.Any<Contact>()).Returns(callInfo =>
        {
            var contact = callInfo.Arg<Contact>();
            return new ContactDto
            {
                Id = contact.Id,
                Name = contact.Name,
                Address = contact.Address,
                PhoneNumber = contact.PhoneNumber,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            };
        });
        
        // Setup mapper to convert CreateContactRequest to Contact
        _mapper.Map<Contact>(Arg.Any<CreateContactRequest>()).Returns(callInfo =>
        {
            var request = callInfo.Arg<CreateContactRequest>();
            return new Contact
            {
                Name = request.Name,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber
            };
        });
        
        _logger = Substitute.For<ILogger<ContactService>>();
        
        _contactService = new ContactService(_fakeRepository, _mapper, _logger);
    }

    [Fact]
    public async Task CreateContactAsync_ValidData_ReturnsContactDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateContactRequest
        {
            Name = "田中太郎",
            Address = "東京都渋谷区",
            PhoneNumber = "03-1234-5678"
        };

        // Act
        var result = await _contactService.CreateContactAsync(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("田中太郎", result.Name);
        Assert.Equal("東京都渋谷区", result.Address);
        Assert.Equal("03-1234-5678", result.PhoneNumber);
    }

    [Fact]
    public async Task CreateContactAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingContact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "田中太郎",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _fakeRepository.CreateAsync(existingContact);

        var request = new CreateContactRequest
        {
            Name = "田中太郎"
        };

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateContactNameException>(
            () => _contactService.CreateContactAsync(userId, request));
    }

    [Fact]
    public async Task GetContactByIdAsync_ExistingContact_ReturnsContactDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "山田花子",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _fakeRepository.CreateAsync(contact);

        // Act
        var result = await _contactService.GetContactByIdAsync(userId, contact.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("山田花子", result.Name);
    }

    [Fact]
    public async Task GetContactByIdAsync_WrongUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "山田花子",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _fakeRepository.CreateAsync(contact);

        // Act
        var result = await _contactService.GetContactByIdAsync(otherUserId, contact.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateContactAsync_ValidData_ReturnsUpdatedContact()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "佐藤次郎",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _fakeRepository.CreateAsync(contact);

        var request = new UpdateContactRequest
        {
            Name = "佐藤次郎",
            Address = "大阪府大阪市",
            PhoneNumber = "06-1234-5678"
        };

        // Act
        var result = await _contactService.UpdateContactAsync(userId, contact.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("佐藤次郎", result.Name);
        Assert.Equal("大阪府大阪市", result.Address);
        Assert.Equal("06-1234-5678", result.PhoneNumber);
    }

    [Fact]
    public async Task DeleteContactAsync_ExistingContact_DeletesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "鈴木一郎",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _fakeRepository.CreateAsync(contact);

        // Act
        await _contactService.DeleteContactAsync(userId, contact.Id);

        // Assert
        var deletedContact = await _fakeRepository.GetByIdAsync(contact.Id);
        Assert.Null(deletedContact);
    }
}

// Fake repository for testing
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
