using AddressBook.Application.DTOs;
using AddressBook.Domain.Entities;
using AddressBook.Infrastructure.Services;
using AddressBook.Tests.TestUtilities;
using AutoMapper;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AddressBook.Tests.Properties;

/// <summary>
/// **Validates: Requirements 2.1**
/// Property 1: 連絡先作成の基本機能
/// 任意の有効な名前（1-100文字）に対して、連絡先作成操作は成功し、データベースに正しく保存される
/// </summary>
public class ContactCreationPropertyTests
{
    /// <summary>
    /// Property 1: Contact creation succeeds for any valid name (1-100 characters)
    /// Tests that contact creation operation succeeds and is correctly saved to the database
    /// for any valid name between 1 and 100 characters.
    /// </summary>
    [Property(MaxTest = 100)]
    public async Task Property1_ContactCreation_SucceedsWithValidName(
        NonEmptyString nameGen,
        string? address,
        string? phoneNumber)
    {
        // Generate valid name (1-100 characters, non-empty after trimming)
        var name = nameGen.Get;
        
        // Filter invalid inputs according to requirements
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return;
        
        // Trim the name to ensure no leading/trailing whitespace
        name = name.Trim();
        
        // After trimming, name must still be non-empty
        if (string.IsNullOrEmpty(name))
            return;
        
        // Validate optional fields
        if (address != null && address.Length > 500)
            return;
        
        if (phoneNumber != null && phoneNumber.Length > 20)
            return;

        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        var createRequest = new CreateContactRequest
        {
            Name = name,
            Address = address,
            PhoneNumber = phoneNumber
        };

        // Act - Create contact
        var createdContact = await contactService.CreateContactAsync(userId, createRequest);
        
        // Assert - Verify contact creation succeeded
        Assert.NotNull(createdContact);
        Assert.NotEqual(Guid.Empty, createdContact.Id);
        Assert.Equal(name, createdContact.Name);
        Assert.Equal(address, createdContact.Address);
        Assert.Equal(phoneNumber, createdContact.PhoneNumber);
        Assert.NotEqual(default(DateTime), createdContact.CreatedAt);
        Assert.NotEqual(default(DateTime), createdContact.UpdatedAt);
        
        // Assert - Verify contact is correctly saved to database
        var retrievedContact = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedContact);
        Assert.Equal(createdContact.Id, retrievedContact.Id);
        Assert.Equal(name, retrievedContact.Name);
        Assert.Equal(address, retrievedContact.Address);
        Assert.Equal(phoneNumber, retrievedContact.PhoneNumber);
        Assert.Equal(createdContact.CreatedAt, retrievedContact.CreatedAt);
        Assert.Equal(createdContact.UpdatedAt, retrievedContact.UpdatedAt);
    }

    /// <summary>
    /// Property 1: Contact creation with minimum valid name (1 character)
    /// Tests edge case of single character names.
    /// </summary>
    [Fact]
    public async Task Property1_ContactCreation_SucceedsWithSingleCharacterName()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        var createRequest = new CreateContactRequest
        {
            Name = "A",
            Address = null,
            PhoneNumber = null
        };

        // Act
        var createdContact = await contactService.CreateContactAsync(userId, createRequest);
        
        // Assert
        Assert.NotNull(createdContact);
        Assert.Equal("A", createdContact.Name);
        
        var retrievedContact = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedContact);
        Assert.Equal("A", retrievedContact.Name);
    }

    /// <summary>
    /// Property 1: Contact creation with maximum valid name (100 characters)
    /// Tests edge case of maximum length names.
    /// </summary>
    [Fact]
    public async Task Property1_ContactCreation_SucceedsWithMaximumLengthName()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        var maxLengthName = new string('あ', 100); // 100 characters
        var createRequest = new CreateContactRequest
        {
            Name = maxLengthName,
            Address = null,
            PhoneNumber = null
        };

        // Act
        var createdContact = await contactService.CreateContactAsync(userId, createRequest);
        
        // Assert
        Assert.NotNull(createdContact);
        Assert.Equal(maxLengthName, createdContact.Name);
        Assert.Equal(100, createdContact.Name.Length);
        
        var retrievedContact = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedContact);
        Assert.Equal(maxLengthName, retrievedContact.Name);
    }

    /// <summary>
    /// Property 1: Contact creation with all optional fields populated
    /// Tests that optional fields (address, phone number) are correctly saved.
    /// </summary>
    [Fact]
    public async Task Property1_ContactCreation_SucceedsWithAllFieldsPopulated()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        var createRequest = new CreateContactRequest
        {
            Name = "田中太郎",
            Address = "東京都渋谷区道玄坂1-2-3",
            PhoneNumber = "03-1234-5678"
        };

        // Act
        var createdContact = await contactService.CreateContactAsync(userId, createRequest);
        
        // Assert
        Assert.NotNull(createdContact);
        Assert.Equal("田中太郎", createdContact.Name);
        Assert.Equal("東京都渋谷区道玄坂1-2-3", createdContact.Address);
        Assert.Equal("03-1234-5678", createdContact.PhoneNumber);
        
        var retrievedContact = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedContact);
        Assert.Equal("田中太郎", retrievedContact.Name);
        Assert.Equal("東京都渋谷区道玄坂1-2-3", retrievedContact.Address);
        Assert.Equal("03-1234-5678", retrievedContact.PhoneNumber);
    }

    /// <summary>
    /// Property 1: Contact creation with only required field (name)
    /// Tests that contacts can be created with only the required name field.
    /// </summary>
    [Fact]
    public async Task Property1_ContactCreation_SucceedsWithOnlyRequiredField()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        var createRequest = new CreateContactRequest
        {
            Name = "山田花子",
            Address = null,
            PhoneNumber = null
        };

        // Act
        var createdContact = await contactService.CreateContactAsync(userId, createRequest);
        
        // Assert
        Assert.NotNull(createdContact);
        Assert.Equal("山田花子", createdContact.Name);
        Assert.Null(createdContact.Address);
        Assert.Null(createdContact.PhoneNumber);
        
        var retrievedContact = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedContact);
        Assert.Equal("山田花子", retrievedContact.Name);
        Assert.Null(retrievedContact.Address);
        Assert.Null(retrievedContact.PhoneNumber);
    }

    /// <summary>
    /// Property 1: Multiple contacts can be created for the same user
    /// Tests that multiple contacts with different names can be created for the same user.
    /// </summary>
    [Fact]
    public async Task Property1_ContactCreation_MultipleContactsForSameUser()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        var names = new[] { "田中太郎", "山田花子", "佐藤次郎", "鈴木一郎", "高橋美咲" };
        var createdContacts = new List<ContactDto>();

        // Act - Create multiple contacts
        foreach (var name in names)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = $"Address for {name}",
                PhoneNumber = "123-456-7890"
            };
            
            var created = await contactService.CreateContactAsync(userId, createRequest);
            createdContacts.Add(created);
        }

        // Assert - Verify all contacts were created and saved correctly
        Assert.Equal(names.Length, createdContacts.Count);
        
        foreach (var created in createdContacts)
        {
            var retrieved = await contactService.GetContactByIdAsync(userId, created.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(created.Name, retrieved.Name);
            Assert.Equal(created.Address, retrieved.Address);
            Assert.Equal(created.PhoneNumber, retrieved.PhoneNumber);
        }
    }

    /// <summary>
    /// Property 1: Contact creation with various character types in name
    /// Tests that names with different character types (Japanese, English, numbers, symbols) work correctly.
    /// </summary>
    [Theory]
    [InlineData("田中太郎")]
    [InlineData("John Smith")]
    [InlineData("山田123")]
    [InlineData("O'Brien")]
    [InlineData("José García")]
    [InlineData("李明")]
    [InlineData("Müller")]
    [InlineData("Владимир")]
    public async Task Property1_ContactCreation_SucceedsWithVariousCharacterTypes(string name)
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        var createRequest = new CreateContactRequest
        {
            Name = name,
            Address = null,
            PhoneNumber = null
        };

        // Act
        var createdContact = await contactService.CreateContactAsync(userId, createRequest);
        
        // Assert
        Assert.NotNull(createdContact);
        Assert.Equal(name, createdContact.Name);
        
        var retrievedContact = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedContact);
        Assert.Equal(name, retrievedContact.Name);
    }

    private static IMapper CreateMapper()
    {
        var mapper = Substitute.For<IMapper>();
        
        mapper.Map<ContactDto>(Arg.Any<Contact>()).Returns(callInfo =>
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
        
        mapper.Map<Contact>(Arg.Any<CreateContactRequest>()).Returns(callInfo =>
        {
            var request = callInfo.Arg<CreateContactRequest>();
            return new Contact
            {
                Name = request.Name,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber
            };
        });
        
        return mapper;
    }
}
