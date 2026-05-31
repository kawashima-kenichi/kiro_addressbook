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
/// **Validates: Requirements 9.1**
/// Property 8: 認可とアクセス制御
/// 任意のユーザーと連絡先の組み合わせに対して、ユーザーは自分が所有する連絡先のみにアクセス可能で、
/// 他のユーザーの連絡先にはアクセスできない
/// </summary>
public class AuthorizationAccessControlPropertyTests
{
    /// <summary>
    /// Property 8: Users can only access their own contacts
    /// Tests that for any combination of users and contacts, a user can only access
    /// contacts they own and cannot access contacts owned by other users.
    /// </summary>
    [Property(MaxTest = 100)]
    public async Task Property8_Authorization_UsersCanOnlyAccessTheirOwnContacts(
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
        
        // Create two different users
        var ownerUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var createRequest = new CreateContactRequest
        {
            Name = name.Trim(),
            Address = address,
            PhoneNumber = phoneNumber
        };

        // Act - Owner creates a contact
        var createdContact = await contactService.CreateContactAsync(ownerUserId, createRequest);
        
        // Assert - Owner can access their own contact (READ)
        var ownerCanRead = await contactService.GetContactByIdAsync(ownerUserId, createdContact.Id);
        Assert.NotNull(ownerCanRead);
        Assert.Equal(createdContact.Id, ownerCanRead.Id);
        Assert.Equal(name.Trim(), ownerCanRead.Name);
        
        // Assert - Other user CANNOT access owner's contact (READ)
        var otherUserCannotRead = await contactService.GetContactByIdAsync(otherUserId, createdContact.Id);
        Assert.Null(otherUserCannotRead);
        
        // Assert - Other user CANNOT update owner's contact (UPDATE)
        var updateRequest = new UpdateContactRequest
        {
            Name = name.Trim(),
            Address = "Unauthorized Update",
            PhoneNumber = "999-999-9999"
        };
        
        await Assert.ThrowsAsync<ContactNotFoundException>(async () =>
        {
            await contactService.UpdateContactAsync(otherUserId, createdContact.Id, updateRequest);
        });
        
        // Verify contact was NOT modified by unauthorized update attempt
        var contactAfterUnauthorizedUpdate = await contactService.GetContactByIdAsync(ownerUserId, createdContact.Id);
        Assert.NotNull(contactAfterUnauthorizedUpdate);
        Assert.Equal(address, contactAfterUnauthorizedUpdate.Address); // Original address unchanged
        Assert.NotEqual("Unauthorized Update", contactAfterUnauthorizedUpdate.Address);
        
        // Assert - Other user CANNOT delete owner's contact (DELETE)
        await Assert.ThrowsAsync<ContactNotFoundException>(async () =>
        {
            await contactService.DeleteContactAsync(otherUserId, createdContact.Id);
        });
        
        // Verify contact still exists after unauthorized delete attempt
        var contactAfterUnauthorizedDelete = await contactService.GetContactByIdAsync(ownerUserId, createdContact.Id);
        Assert.NotNull(contactAfterUnauthorizedDelete);
        
        // Assert - Owner CAN update their own contact (UPDATE)
        var ownerUpdateRequest = new UpdateContactRequest
        {
            Name = name.Trim(),
            Address = "Owner's Update",
            PhoneNumber = "111-111-1111"
        };
        
        var ownerUpdatedContact = await contactService.UpdateContactAsync(ownerUserId, createdContact.Id, ownerUpdateRequest);
        Assert.NotNull(ownerUpdatedContact);
        Assert.Equal("Owner's Update", ownerUpdatedContact.Address);
        Assert.Equal("111-111-1111", ownerUpdatedContact.PhoneNumber);
        
        // Assert - Owner CAN delete their own contact (DELETE)
        await contactService.DeleteContactAsync(ownerUserId, createdContact.Id);
        
        var contactAfterOwnerDelete = await contactService.GetContactByIdAsync(ownerUserId, createdContact.Id);
        Assert.Null(contactAfterOwnerDelete);
    }

    /// <summary>
    /// Property 8: Users can only list their own contacts
    /// Tests that GetContactsAsync only returns contacts owned by the requesting user.
    /// </summary>
    [Fact]
    public async Task Property8_Authorization_UsersCanOnlyListTheirOwnContacts()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        
        // Create contacts for user1
        var user1Contacts = new List<ContactDto>();
        for (int i = 0; i < 3; i++)
        {
            var contact = await contactService.CreateContactAsync(user1Id, new CreateContactRequest
            {
                Name = $"User1 Contact {i}",
                Address = $"Address {i}",
                PhoneNumber = "123-456-7890"
            });
            user1Contacts.Add(contact);
        }
        
        // Create contacts for user2
        var user2Contacts = new List<ContactDto>();
        for (int i = 0; i < 2; i++)
        {
            var contact = await contactService.CreateContactAsync(user2Id, new CreateContactRequest
            {
                Name = $"User2 Contact {i}",
                Address = $"Address {i}",
                PhoneNumber = "098-765-4321"
            });
            user2Contacts.Add(contact);
        }
        
        // Act - User1 lists their contacts
        var (user1List, user1TotalCount) = await contactService.GetContactsAsync(user1Id, 1, 50);
        
        // Assert - User1 only sees their own contacts
        Assert.Equal(3, user1TotalCount);
        Assert.Equal(3, user1List.Count());
        Assert.All(user1List, contact => Assert.StartsWith("User1 Contact", contact.Name));
        
        // Act - User2 lists their contacts
        var (user2List, user2TotalCount) = await contactService.GetContactsAsync(user2Id, 1, 50);
        
        // Assert - User2 only sees their own contacts
        Assert.Equal(2, user2TotalCount);
        Assert.Equal(2, user2List.Count());
        Assert.All(user2List, contact => Assert.StartsWith("User2 Contact", contact.Name));
        
        // Assert - No cross-contamination between users
        var user1ContactIds = user1List.Select(c => c.Id).ToHashSet();
        var user2ContactIds = user2List.Select(c => c.Id).ToHashSet();
        Assert.Empty(user1ContactIds.Intersect(user2ContactIds));
    }

    /// <summary>
    /// Property 8: Multiple users with same contact names are isolated
    /// Tests that even when multiple users have contacts with the same name,
    /// they cannot access each other's contacts.
    /// </summary>
    [Fact]
    public async Task Property8_Authorization_MultipleUsersWithSameContactNamesAreIsolated()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var user3Id = Guid.NewGuid();
        
        var sharedContactName = "田中太郎";
        
        // Act - All three users create a contact with the same name
        var user1Contact = await contactService.CreateContactAsync(user1Id, new CreateContactRequest
        {
            Name = sharedContactName,
            Address = "User1's Address",
            PhoneNumber = "111-111-1111"
        });
        
        var user2Contact = await contactService.CreateContactAsync(user2Id, new CreateContactRequest
        {
            Name = sharedContactName,
            Address = "User2's Address",
            PhoneNumber = "222-222-2222"
        });
        
        var user3Contact = await contactService.CreateContactAsync(user3Id, new CreateContactRequest
        {
            Name = sharedContactName,
            Address = "User3's Address",
            PhoneNumber = "333-333-3333"
        });
        
        // Assert - Each user can only access their own contact
        var user1Retrieved = await contactService.GetContactByIdAsync(user1Id, user1Contact.Id);
        Assert.NotNull(user1Retrieved);
        Assert.Equal("User1's Address", user1Retrieved.Address);
        Assert.Equal("111-111-1111", user1Retrieved.PhoneNumber);
        
        var user2Retrieved = await contactService.GetContactByIdAsync(user2Id, user2Contact.Id);
        Assert.NotNull(user2Retrieved);
        Assert.Equal("User2's Address", user2Retrieved.Address);
        Assert.Equal("222-222-2222", user2Retrieved.PhoneNumber);
        
        var user3Retrieved = await contactService.GetContactByIdAsync(user3Id, user3Contact.Id);
        Assert.NotNull(user3Retrieved);
        Assert.Equal("User3's Address", user3Retrieved.Address);
        Assert.Equal("333-333-3333", user3Retrieved.PhoneNumber);
        
        // Assert - User1 cannot access User2's or User3's contacts
        Assert.Null(await contactService.GetContactByIdAsync(user1Id, user2Contact.Id));
        Assert.Null(await contactService.GetContactByIdAsync(user1Id, user3Contact.Id));
        
        // Assert - User2 cannot access User1's or User3's contacts
        Assert.Null(await contactService.GetContactByIdAsync(user2Id, user1Contact.Id));
        Assert.Null(await contactService.GetContactByIdAsync(user2Id, user3Contact.Id));
        
        // Assert - User3 cannot access User1's or User2's contacts
        Assert.Null(await contactService.GetContactByIdAsync(user3Id, user1Contact.Id));
        Assert.Null(await contactService.GetContactByIdAsync(user3Id, user2Contact.Id));
        
        // Assert - Each user's list only contains their own contact
        var (user1List, _) = await contactService.GetContactsAsync(user1Id, 1, 50);
        Assert.Single(user1List);
        Assert.Equal(user1Contact.Id, user1List.First().Id);
        
        var (user2List, _) = await contactService.GetContactsAsync(user2Id, 1, 50);
        Assert.Single(user2List);
        Assert.Equal(user2Contact.Id, user2List.First().Id);
        
        var (user3List, _) = await contactService.GetContactsAsync(user3Id, 1, 50);
        Assert.Single(user3List);
        Assert.Equal(user3Contact.Id, user3List.First().Id);
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
        
        mapper.Map<IEnumerable<ContactDto>>(Arg.Any<IEnumerable<Contact>>()).Returns(callInfo =>
        {
            var contacts = callInfo.Arg<IEnumerable<Contact>>();
            return contacts.Select(c => new ContactDto
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });
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
