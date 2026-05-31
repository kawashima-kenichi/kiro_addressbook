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
/// **Validates: Requirements 2.4, 8.2**
/// Property 2: 連絡先名の重複防止
/// 任意のユーザーと既存の連絡先名に対して、同じ名前（大文字小文字を区別しない）で
/// 新しい連絡先を作成しようとすると、システムは拒否し、
/// 「この名前の連絡先は既に存在します」エラーを表示する
/// </summary>
public class DuplicateContactNamePropertyTests
{
    /// <summary>
    /// Property 2: Duplicate contact name prevention (case-insensitive)
    /// Tests that for any user and existing contact name, attempting to create
    /// a new contact with the same name (case-insensitive) is rejected with
    /// the error message "この名前の連絡先は既に存在します"
    /// </summary>
    [Property(MaxTest = 100)]
    public async Task Property2_DuplicateNamePrevention_RejectsDuplicateNamesIgnoringCase(
        string existingName,
        string? address1,
        string? phoneNumber1,
        string? address2,
        string? phoneNumber2)
    {
        // Filter invalid inputs for the existing contact name
        if (string.IsNullOrWhiteSpace(existingName) || existingName.Length > 100)
            return;
        
        if (address1 != null && address1.Length > 500)
            return;
        
        if (phoneNumber1 != null && phoneNumber1.Length > 20)
            return;
        
        if (address2 != null && address2.Length > 500)
            return;
        
        if (phoneNumber2 != null && phoneNumber2.Length > 20)
            return;

        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();
        
        // Create the first contact with the existing name
        var firstContactRequest = new CreateContactRequest
        {
            Name = existingName.Trim(),
            Address = address1,
            PhoneNumber = phoneNumber1
        };

        var firstContact = await contactService.CreateContactAsync(userId, firstContactRequest);
        Assert.NotNull(firstContact);
        
        // Generate case variations of the existing name
        var caseVariations = GenerateCaseVariations(existingName.Trim());
        
        // Act & Assert - Try to create contacts with each case variation
        foreach (var nameVariation in caseVariations)
        {
            var duplicateRequest = new CreateContactRequest
            {
                Name = nameVariation,
                Address = address2,
                PhoneNumber = phoneNumber2
            };

            // Assert that duplicate name is rejected
            var exception = await Assert.ThrowsAsync<DuplicateContactNameException>(
                () => contactService.CreateContactAsync(userId, duplicateRequest));
            
            // Verify the error message
            Assert.Equal("この名前の連絡先は既に存在します", exception.Message);
        }
        
        // Verify that the original contact still exists and is unchanged
        var retrievedContact = await contactService.GetContactByIdAsync(userId, firstContact.Id);
        Assert.NotNull(retrievedContact);
        Assert.Equal(existingName.Trim(), retrievedContact.Name);
        Assert.Equal(address1, retrievedContact.Address);
        Assert.Equal(phoneNumber1, retrievedContact.PhoneNumber);
    }

    /// <summary>
    /// Property 2: Duplicate name prevention across different users
    /// Tests that duplicate name checking is scoped to individual users,
    /// allowing different users to have contacts with the same name.
    /// </summary>
    [Property(MaxTest = 100)]
    public async Task Property2_DuplicateNamePrevention_AllowsSameNameForDifferentUsers(
        string contactName,
        string? address1,
        string? phoneNumber1,
        string? address2,
        string? phoneNumber2)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(contactName) || contactName.Length > 100)
            return;
        
        if (address1 != null && address1.Length > 500)
            return;
        
        if (phoneNumber1 != null && phoneNumber1.Length > 20)
            return;
        
        if (address2 != null && address2.Length > 500)
            return;
        
        if (phoneNumber2 != null && phoneNumber2.Length > 20)
            return;

        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        
        // Act - Create contact with same name for user 1
        var request1 = new CreateContactRequest
        {
            Name = contactName.Trim(),
            Address = address1,
            PhoneNumber = phoneNumber1
        };
        var contact1 = await contactService.CreateContactAsync(user1Id, request1);
        
        // Act - Create contact with same name for user 2 (should succeed)
        var request2 = new CreateContactRequest
        {
            Name = contactName.Trim(),
            Address = address2,
            PhoneNumber = phoneNumber2
        };
        var contact2 = await contactService.CreateContactAsync(user2Id, request2);
        
        // Assert - Both contacts should exist with the same name but different users
        Assert.NotNull(contact1);
        Assert.NotNull(contact2);
        Assert.Equal(contactName.Trim(), contact1.Name);
        Assert.Equal(contactName.Trim(), contact2.Name);
        Assert.NotEqual(contact1.Id, contact2.Id);
        
        // Verify each user can only see their own contact
        var retrievedContact1 = await contactService.GetContactByIdAsync(user1Id, contact1.Id);
        var retrievedContact2 = await contactService.GetContactByIdAsync(user2Id, contact2.Id);
        
        Assert.NotNull(retrievedContact1);
        Assert.NotNull(retrievedContact2);
        
        // Verify user 1 cannot access user 2's contact and vice versa
        var unauthorizedAccess1 = await contactService.GetContactByIdAsync(user1Id, contact2.Id);
        var unauthorizedAccess2 = await contactService.GetContactByIdAsync(user2Id, contact1.Id);
        
        Assert.Null(unauthorizedAccess1);
        Assert.Null(unauthorizedAccess2);
    }

    /// <summary>
    /// Property 2: Duplicate name prevention on update
    /// Tests that when updating a contact, the system prevents changing the name
    /// to match another existing contact's name (case-insensitive).
    /// </summary>
    [Property(MaxTest = 100)]
    public async Task Property2_DuplicateNamePrevention_RejectsDuplicateNamesOnUpdate(
        string name1,
        string name2,
        string? address,
        string? phoneNumber)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(name1) || name1.Length > 100)
            return;
        
        if (string.IsNullOrWhiteSpace(name2) || name2.Length > 100)
            return;
        
        // Skip if names are the same (case-insensitive) - not a duplicate scenario
        if (name1.Trim().Equals(name2.Trim(), StringComparison.OrdinalIgnoreCase))
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
        
        // Create two contacts with different names
        var contact1Request = new CreateContactRequest
        {
            Name = name1.Trim(),
            Address = address,
            PhoneNumber = phoneNumber
        };
        var contact1 = await contactService.CreateContactAsync(userId, contact1Request);
        
        var contact2Request = new CreateContactRequest
        {
            Name = name2.Trim(),
            Address = address,
            PhoneNumber = phoneNumber
        };
        var contact2 = await contactService.CreateContactAsync(userId, contact2Request);
        
        // Generate case variations of name1
        var caseVariations = GenerateCaseVariations(name1.Trim());
        
        // Act & Assert - Try to update contact2's name to match contact1's name (with case variations)
        foreach (var nameVariation in caseVariations)
        {
            var updateRequest = new UpdateContactRequest
            {
                Name = nameVariation,
                Address = address,
                PhoneNumber = phoneNumber
            };

            // Assert that duplicate name on update is rejected
            var exception = await Assert.ThrowsAsync<DuplicateContactNameException>(
                () => contactService.UpdateContactAsync(userId, contact2.Id, updateRequest));
            
            // Verify the error message
            Assert.Equal("この名前の連絡先は既に存在します", exception.Message);
        }
        
        // Verify that contact2 still has its original name
        var retrievedContact2 = await contactService.GetContactByIdAsync(userId, contact2.Id);
        Assert.NotNull(retrievedContact2);
        Assert.Equal(name2.Trim(), retrievedContact2.Name);
    }

    /// <summary>
    /// Property 2: Updating contact with same name (case change only) should succeed
    /// Tests that updating a contact's name to the same name with different casing
    /// (e.g., "John Doe" to "JOHN DOE") should succeed since it's the same contact.
    /// </summary>
    [Property(MaxTest = 100)]
    public async Task Property2_DuplicateNamePrevention_AllowsSameContactNameWithCaseChange(
        string originalName,
        string? address,
        string? phoneNumber)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(originalName) || originalName.Length > 100)
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
        
        // Create a contact
        var createRequest = new CreateContactRequest
        {
            Name = originalName.Trim(),
            Address = address,
            PhoneNumber = phoneNumber
        };
        var contact = await contactService.CreateContactAsync(userId, createRequest);
        
        // Generate case variations of the same name
        var caseVariations = GenerateCaseVariations(originalName.Trim());
        
        // Act & Assert - Update the contact with case variations of its own name (should succeed)
        foreach (var nameVariation in caseVariations)
        {
            var updateRequest = new UpdateContactRequest
            {
                Name = nameVariation,
                Address = address,
                PhoneNumber = phoneNumber
            };

            // This should succeed because it's the same contact
            var updatedContact = await contactService.UpdateContactAsync(userId, contact.Id, updateRequest);
            
            Assert.NotNull(updatedContact);
            Assert.Equal(nameVariation, updatedContact.Name);
            Assert.Equal(contact.Id, updatedContact.Id);
        }
    }

    /// <summary>
    /// Generates case variations of a given name for testing case-insensitive duplicate detection
    /// </summary>
    private static List<string> GenerateCaseVariations(string name)
    {
        var variations = new List<string>
        {
            name,                           // Original
            name.ToLower(),                 // All lowercase
            name.ToUpper(),                 // All uppercase
        };
        
        // Add mixed case variation if name has multiple characters
        if (name.Length > 1)
        {
            var chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = i % 2 == 0 ? char.ToUpper(chars[i]) : char.ToLower(chars[i]);
            }
            variations.Add(new string(chars));
        }
        
        // Remove duplicates and return
        return variations.Distinct().ToList();
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
