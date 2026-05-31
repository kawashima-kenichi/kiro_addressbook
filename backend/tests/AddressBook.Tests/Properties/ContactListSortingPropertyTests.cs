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
/// **Validates: Requirements 3.1**
/// Property 4: 連絡先リストのソート機能
/// 任意の連絡先の集合に対して、連絡先リストは名前のアルファベット順（大文字小文字を区別しない）で
/// 正しくソートされて表示される
/// </summary>
public class ContactListSortingPropertyTests
{
    /// <summary>
    /// Property 4: Contact list is always sorted alphabetically by name (case-insensitive)
    /// Tests that for any collection of contacts, the contact list is correctly sorted
    /// in alphabetical order by name, ignoring case.
    /// This test focuses on ASCII letters to avoid Unicode sorting complexity.
    /// </summary>
    [Property(MaxTest = 100)]
    public async Task Property4_ContactListSorting_AlwaysSortedAlphabeticallyIgnoringCase(
        NonEmptyArray<NonEmptyString> contactNames)
    {
        // Filter to ensure valid contact names (1-100 characters)
        // Only allow ASCII letters, digits, spaces, and common punctuation to avoid Unicode sorting issues
        var validNames = contactNames.Get
            .Select(n => n.Get.Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n) && 
                       n.Length >= 1 && 
                       n.Length <= 100 &&
                       n.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || 
                                  (c >= '0' && c <= '9') || c == ' ' || c == '-' || c == '\''))
            .Distinct(StringComparer.OrdinalIgnoreCase) // Ensure unique names (case-insensitive)
            .ToList();

        // Skip if we don't have at least 2 contacts to sort
        if (validNames.Count < 2)
            return;

        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();

        // Create contacts with the generated names
        var createdContacts = new List<ContactDto>();
        foreach (var name in validNames)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = null,
                PhoneNumber = null
            };
            var contact = await contactService.CreateContactAsync(userId, createRequest);
            createdContacts.Add(contact);
        }

        // Act - Retrieve all contacts (using large page size to get all contacts)
        var (retrievedContacts, totalCount) = await contactService.GetContactsAsync(
            userId, page: 1, pageSize: validNames.Count);

        var contactList = retrievedContacts.ToList();

        // Assert - Verify the list is sorted alphabetically (case-insensitive)
        Assert.Equal(validNames.Count, contactList.Count);
        Assert.Equal(validNames.Count, totalCount);

        // Verify sorting: each contact should be <= the next contact (case-insensitive)
        for (int i = 0; i < contactList.Count - 1; i++)
        {
            var currentName = contactList[i].Name;
            var nextName = contactList[i + 1].Name;
            
            var comparison = string.Compare(
                currentName, 
                nextName, 
                StringComparison.OrdinalIgnoreCase);
            
            Assert.True(comparison <= 0, 
                $"Contact list is not sorted correctly: '{currentName}' should come before or equal to '{nextName}'");
        }

        // Verify the list matches the expected sorted order
        var expectedSortedNames = validNames
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (int i = 0; i < contactList.Count; i++)
        {
            Assert.Equal(
                expectedSortedNames[i], 
                contactList[i].Name, 
                ignoreCase: true);
        }
    }

    /// <summary>
    /// Property 4: Contact list sorting with mixed case names
    /// Tests that sorting is case-insensitive by creating contacts with various case combinations.
    /// </summary>
    [Fact]
    public async Task Property4_ContactListSorting_CaseInsensitiveSorting()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();

        // Create contacts with mixed case variations of similar names
        var names = new[]
        {
            "alice",
            "CHARLIE",
            "bob",
            "ALICE2",
            "Charlie2",
            "BOB2"
        };

        foreach (var name in names)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = null,
                PhoneNumber = null
            };
            await contactService.CreateContactAsync(userId, createRequest);
        }

        // Act
        var (retrievedContacts, _) = await contactService.GetContactsAsync(
            userId, page: 1, pageSize: 50);

        var contactList = retrievedContacts.ToList();

        // Assert - Verify case-insensitive alphabetical order
        var expectedOrder = new[] { "alice", "ALICE2", "bob", "BOB2", "CHARLIE", "Charlie2" };
        
        Assert.Equal(expectedOrder.Length, contactList.Count);
        
        for (int i = 0; i < expectedOrder.Length; i++)
        {
            Assert.Equal(expectedOrder[i], contactList[i].Name, ignoreCase: true);
        }
    }

    /// <summary>
    /// Property 4: Contact list sorting with Japanese characters
    /// Tests that sorting works correctly and consistently with Japanese names.
    /// Note: Japanese character sorting follows Unicode ordering.
    /// This test verifies that the sort is consistent (monotonic), not a specific order.
    /// </summary>
    [Fact]
    public async Task Property4_ContactListSorting_JapaneseCharacters()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();

        var names = new[]
        {
            "田中太郎",
            "山田花子",
            "佐藤次郎",
            "鈴木一郎",
            "高橋美咲"
        };

        foreach (var name in names)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = null,
                PhoneNumber = null
            };
            await contactService.CreateContactAsync(userId, createRequest);
        }

        // Act
        var (retrievedContacts, _) = await contactService.GetContactsAsync(
            userId, page: 1, pageSize: 50);

        var contactList = retrievedContacts.ToList();

        // Assert - Verify all contacts are retrieved
        Assert.Equal(names.Length, contactList.Count);

        // Verify sorting is consistent (monotonic - each element <= next element)
        for (int i = 0; i < contactList.Count - 1; i++)
        {
            var currentName = contactList[i].Name;
            var nextName = contactList[i + 1].Name;
            var comparison = string.Compare(
                currentName,
                nextName,
                StringComparison.OrdinalIgnoreCase);
            
            Assert.True(comparison <= 0,
                $"Japanese names not sorted consistently: '{currentName}' (comparison={comparison}) should come before or equal to '{nextName}'");
        }
    }

    /// <summary>
    /// Property 4: Contact list sorting with special characters
    /// Tests that sorting works correctly and consistently with names containing special characters.
    /// Note: Special character sorting follows Unicode ordering.
    /// </summary>
    [Fact]
    public async Task Property4_ContactListSorting_SpecialCharacters()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();

        var names = new[]
        {
            "O'Brien",
            "McDonald",
            "Müller",
            "José García",
            "李明",
            "Владимир"
        };

        foreach (var name in names)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = null,
                PhoneNumber = null
            };
            await contactService.CreateContactAsync(userId, createRequest);
        }

        // Act
        var (retrievedContacts, _) = await contactService.GetContactsAsync(
            userId, page: 1, pageSize: 50);

        var contactList = retrievedContacts.ToList();

        // Assert - Verify all contacts are retrieved
        Assert.Equal(names.Length, contactList.Count);

        // Verify sorting is consistent (monotonic - each element <= next element)
        for (int i = 0; i < contactList.Count - 1; i++)
        {
            var currentName = contactList[i].Name;
            var nextName = contactList[i + 1].Name;
            var comparison = string.Compare(
                currentName,
                nextName,
                StringComparison.OrdinalIgnoreCase);
            
            Assert.True(comparison <= 0,
                $"Special character names not sorted consistently: '{currentName}' (comparison={comparison}) should come before or equal to '{nextName}'");
        }
    }

    /// <summary>
    /// Property 4: Contact list sorting with numbers in names
    /// Tests that sorting works correctly with names containing numbers.
    /// </summary>
    [Fact]
    public async Task Property4_ContactListSorting_NamesWithNumbers()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();

        var names = new[]
        {
            "Contact 1",
            "Contact 10",
            "Contact 2",
            "Contact 20",
            "Contact 3"
        };

        foreach (var name in names)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = null,
                PhoneNumber = null
            };
            await contactService.CreateContactAsync(userId, createRequest);
        }

        // Act
        var (retrievedContacts, _) = await contactService.GetContactsAsync(
            userId, page: 1, pageSize: 50);

        var contactList = retrievedContacts.ToList();

        // Assert - Verify all contacts are retrieved
        Assert.Equal(names.Length, contactList.Count);

        // Verify sorting order (lexicographic, not numeric)
        for (int i = 0; i < contactList.Count - 1; i++)
        {
            var comparison = string.Compare(
                contactList[i].Name,
                contactList[i + 1].Name,
                StringComparison.OrdinalIgnoreCase);
            
            Assert.True(comparison <= 0,
                $"Names with numbers not sorted correctly: '{contactList[i].Name}' should come before '{contactList[i + 1].Name}'");
        }
    }

    /// <summary>
    /// Property 4: Contact list sorting with pagination
    /// Tests that sorting is maintained across paginated results.
    /// </summary>
    [Fact]
    public async Task Property4_ContactListSorting_MaintainsSortingAcrossPagination()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();

        // Create 10 contacts
        var names = new[]
        {
            "Alice", "Bob", "Charlie", "David", "Eve",
            "Frank", "Grace", "Henry", "Ivy", "Jack"
        };

        foreach (var name in names)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = null,
                PhoneNumber = null
            };
            await contactService.CreateContactAsync(userId, createRequest);
        }

        // Act - Get contacts in pages of 3
        var page1 = await contactService.GetContactsAsync(userId, page: 1, pageSize: 3);
        var page2 = await contactService.GetContactsAsync(userId, page: 2, pageSize: 3);
        var page3 = await contactService.GetContactsAsync(userId, page: 3, pageSize: 3);
        var page4 = await contactService.GetContactsAsync(userId, page: 4, pageSize: 3);

        // Combine all pages
        var allContacts = page1.Contacts
            .Concat(page2.Contacts)
            .Concat(page3.Contacts)
            .Concat(page4.Contacts)
            .ToList();

        // Assert - Verify total count
        Assert.Equal(10, page1.TotalCount);
        Assert.Equal(10, allContacts.Count);

        // Verify sorting across all pages
        for (int i = 0; i < allContacts.Count - 1; i++)
        {
            var comparison = string.Compare(
                allContacts[i].Name,
                allContacts[i + 1].Name,
                StringComparison.OrdinalIgnoreCase);
            
            Assert.True(comparison <= 0,
                $"Sorting not maintained across pagination: '{allContacts[i].Name}' should come before '{allContacts[i + 1].Name}'");
        }

        // Verify expected order
        var expectedOrder = names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        for (int i = 0; i < allContacts.Count; i++)
        {
            Assert.Equal(expectedOrder[i], allContacts[i].Name, ignoreCase: true);
        }
    }

    /// <summary>
    /// Property 4: Contact list sorting with single contact
    /// Tests edge case of a single contact (trivially sorted).
    /// </summary>
    [Fact]
    public async Task Property4_ContactListSorting_SingleContact()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();

        var createRequest = new CreateContactRequest
        {
            Name = "Single Contact",
            Address = null,
            PhoneNumber = null
        };
        await contactService.CreateContactAsync(userId, createRequest);

        // Act
        var (retrievedContacts, totalCount) = await contactService.GetContactsAsync(
            userId, page: 1, pageSize: 50);

        var contactList = retrievedContacts.ToList();

        // Assert
        Assert.Single(contactList);
        Assert.Equal(1, totalCount);
        Assert.Equal("Single Contact", contactList[0].Name);
    }

    /// <summary>
    /// Property 4: Contact list sorting with empty list
    /// Tests edge case of no contacts (empty list is trivially sorted).
    /// </summary>
    [Fact]
    public async Task Property4_ContactListSorting_EmptyList()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        var userId = Guid.NewGuid();

        // Act - No contacts created
        var (retrievedContacts, totalCount) = await contactService.GetContactsAsync(
            userId, page: 1, pageSize: 50);

        var contactList = retrievedContacts.ToList();

        // Assert
        Assert.Empty(contactList);
        Assert.Equal(0, totalCount);
    }

    /// <summary>
    /// Property 4: Contact list sorting is isolated per user
    /// Tests that sorting is applied independently for each user's contact list.
    /// </summary>
    [Fact]
    public async Task Property4_ContactListSorting_IsolatedPerUser()
    {
        // Arrange
        var mapper = CreateMapper();
        var logger = Substitute.For<ILogger<ContactService>>();
        var repository = new FakeContactRepository();
        var contactService = new ContactService(repository, mapper, logger);
        
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        // Create contacts for user 1
        var user1Names = new[] { "Zoe", "Alice", "Mike" };
        foreach (var name in user1Names)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = null,
                PhoneNumber = null
            };
            await contactService.CreateContactAsync(user1Id, createRequest);
        }

        // Create contacts for user 2
        var user2Names = new[] { "Bob", "Yuki", "David" };
        foreach (var name in user2Names)
        {
            var createRequest = new CreateContactRequest
            {
                Name = name,
                Address = null,
                PhoneNumber = null
            };
            await contactService.CreateContactAsync(user2Id, createRequest);
        }

        // Act
        var (user1Contacts, user1TotalCount) = await contactService.GetContactsAsync(
            user1Id, page: 1, pageSize: 50);
        var (user2Contacts, user2TotalCount) = await contactService.GetContactsAsync(
            user2Id, page: 1, pageSize: 50);

        var user1List = user1Contacts.ToList();
        var user2List = user2Contacts.ToList();

        // Assert - Verify each user has their own sorted list
        Assert.Equal(3, user1TotalCount);
        Assert.Equal(3, user2TotalCount);

        // Verify user 1's list is sorted
        var expectedUser1Order = new[] { "Alice", "Mike", "Zoe" };
        for (int i = 0; i < user1List.Count; i++)
        {
            Assert.Equal(expectedUser1Order[i], user1List[i].Name, ignoreCase: true);
        }

        // Verify user 2's list is sorted
        var expectedUser2Order = new[] { "Bob", "David", "Yuki" };
        for (int i = 0; i < user2List.Count; i++)
        {
            Assert.Equal(expectedUser2Order[i], user2List[i].Name, ignoreCase: true);
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
