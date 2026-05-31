using AddressBook.Application.DTOs;
using AddressBook.Domain.Entities;
using AddressBook.Infrastructure.Services;
using AddressBook.Tests.TestUtilities;
using AutoMapper;
using FsCheck.Xunit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AddressBook.Tests.Properties;

/// <summary>
/// **Validates: Requirements 6.1**
/// Property 7: データ永続化の保証
/// 任意の連絡先データに対して、明示的に削除されるまで、
/// すべての連絡先情報（名前、住所、電話番号、作成日、最終更新日）が永続的に保持される
/// </summary>
public class DataPersistencePropertyTests
{
    /// <summary>
    /// Property 7: Contact data persists until explicitly deleted
    /// Tests that all contact information (name, address, phone, timestamps) 
    /// is permanently retained until explicitly deleted by the user.
    /// </summary>
    [Property(MaxTest = 100)]
    public async Task Property7_DataPersistence_ContactDataPersistedUntilExplicitlyDeleted(
        string name,
        string? address,
        string? phoneNumber)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return;
        
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
            Name = name.Trim(),
            Address = address,
            PhoneNumber = phoneNumber
        };

        // Act - Create contact
        var createdContact = await contactService.CreateContactAsync(userId, createRequest);
        
        // Assert - Verify all data is persisted immediately after creation
        var retrievedAfterCreate = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedAfterCreate);
        Assert.Equal(name.Trim(), retrievedAfterCreate.Name);
        Assert.Equal(address, retrievedAfterCreate.Address);
        Assert.Equal(phoneNumber, retrievedAfterCreate.PhoneNumber);
        Assert.NotEqual(default(DateTime), retrievedAfterCreate.CreatedAt);
        Assert.NotEqual(default(DateTime), retrievedAfterCreate.UpdatedAt);
        
        // Store original timestamps
        var originalCreatedAt = retrievedAfterCreate.CreatedAt;
        var originalUpdatedAt = retrievedAfterCreate.UpdatedAt;

        // Act - Simulate time passing and multiple retrievals (data should persist)
        await Task.Delay(10);
        
        var retrievedAgain1 = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedAgain1);
        Assert.Equal(name.Trim(), retrievedAgain1.Name);
        Assert.Equal(address, retrievedAgain1.Address);
        Assert.Equal(phoneNumber, retrievedAgain1.PhoneNumber);
        Assert.Equal(originalCreatedAt, retrievedAgain1.CreatedAt);
        Assert.Equal(originalUpdatedAt, retrievedAgain1.UpdatedAt);
        
        await Task.Delay(10);
        
        var retrievedAgain2 = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedAgain2);
        Assert.Equal(name.Trim(), retrievedAgain2.Name);
        Assert.Equal(address, retrievedAgain2.Address);
        Assert.Equal(phoneNumber, retrievedAgain2.PhoneNumber);
        Assert.Equal(originalCreatedAt, retrievedAgain2.CreatedAt);
        Assert.Equal(originalUpdatedAt, retrievedAgain2.UpdatedAt);

        // Act - Update contact (UpdatedAt should change, but CreatedAt should remain)
        var updateRequest = new UpdateContactRequest
        {
            Name = name.Trim(),
            Address = address ?? "Updated Address",
            PhoneNumber = phoneNumber ?? "123-456-7890"
        };
        
        await Task.Delay(10);
        var updatedContact = await contactService.UpdateContactAsync(userId, createdContact.Id, updateRequest);
        
        // Assert - Verify data persists after update with correct timestamps
        Assert.NotNull(updatedContact);
        Assert.Equal(name.Trim(), updatedContact.Name);
        Assert.Equal(updateRequest.Address, updatedContact.Address);
        Assert.Equal(updateRequest.PhoneNumber, updatedContact.PhoneNumber);
        Assert.Equal(originalCreatedAt, updatedContact.CreatedAt); // CreatedAt should NOT change
        Assert.True(updatedContact.UpdatedAt >= originalUpdatedAt); // UpdatedAt should be updated
        
        // Act - Retrieve after update
        var retrievedAfterUpdate = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.NotNull(retrievedAfterUpdate);
        Assert.Equal(name.Trim(), retrievedAfterUpdate.Name);
        Assert.Equal(updateRequest.Address, retrievedAfterUpdate.Address);
        Assert.Equal(updateRequest.PhoneNumber, retrievedAfterUpdate.PhoneNumber);
        Assert.Equal(originalCreatedAt, retrievedAfterUpdate.CreatedAt);
        Assert.True(retrievedAfterUpdate.UpdatedAt >= originalUpdatedAt);

        // Act - Explicitly delete contact
        await contactService.DeleteContactAsync(userId, createdContact.Id);
        
        // Assert - Verify data is removed after explicit deletion
        var retrievedAfterDelete = await contactService.GetContactByIdAsync(userId, createdContact.Id);
        Assert.Null(retrievedAfterDelete);
    }

    /// <summary>
    /// Property 7: Multiple contacts persist independently
    /// Tests that multiple contacts can be created and persist independently,
    /// and deleting one contact does not affect others.
    /// </summary>
    [Fact]
    public async Task Property7_DataPersistence_MultipleContactsPersistIndependently()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        var testNames = new[] { "田中太郎", "山田花子", "佐藤次郎", "鈴木一郎", "高橋美咲" };
        var createdContacts = new List<ContactDto>();

        // Act - Create multiple contacts
        foreach (var name in testNames)
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

        // Assert - Verify all contacts persist independently
        foreach (var created in createdContacts)
        {
            var retrieved = await contactService.GetContactByIdAsync(userId, created.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(created.Name, retrieved.Name);
            Assert.Equal(created.Address, retrieved.Address);
            Assert.Equal(created.PhoneNumber, retrieved.PhoneNumber);
            Assert.Equal(created.CreatedAt, retrieved.CreatedAt);
            Assert.Equal(created.UpdatedAt, retrieved.UpdatedAt);
        }

        // Act - Delete one contact
        var contactToDelete = createdContacts[0];
        await contactService.DeleteContactAsync(userId, contactToDelete.Id);
        
        // Assert - Verify deleted contact is gone
        var deletedContact = await contactService.GetContactByIdAsync(userId, contactToDelete.Id);
        Assert.Null(deletedContact);
        
        // Assert - Verify other contacts still persist
        foreach (var otherContact in createdContacts.Skip(1))
        {
            var retrieved = await contactService.GetContactByIdAsync(userId, otherContact.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(otherContact.Name, retrieved.Name);
        }
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
