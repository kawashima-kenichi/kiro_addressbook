using System.Net;
using System.Net.Http.Json;
using AddressBook.Application.DTOs;

namespace AddressBook.Tests.Integration;

/// <summary>
/// アクセス制御統合テスト
/// 要件: 9.1 (認可とアクセス制御 - ユーザーは自分の連絡先のみにアクセス可能)
/// </summary>
public class AccessControlIntegrationTests : IntegrationTestBase
{
    public AccessControlIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetContacts_OnlyReturnsOwnContacts()
    {
        // Arrange - Create two users with their own contacts
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        // User 1 creates contacts
        await CreateTestContactAsync(token1, name: "ユーザー1の連絡先A");
        await CreateTestContactAsync(token1, name: "ユーザー1の連絡先B");

        // User 2 creates contacts
        await CreateTestContactAsync(token2, name: "ユーザー2の連絡先A");

        // Act - User 1 gets their contacts
        SetAuthorizationHeader(token1);
        var response = await Client.GetAsync("/api/contacts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactListResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Contacts.Count);
        Assert.All(result.Contacts, contact => 
            Assert.StartsWith("ユーザー1", contact.Name));
    }

    [Fact]
    public async Task GetContactById_OtherUsersContact_ReturnsNotFound()
    {
        // Arrange - Create two users
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        // User 1 creates a contact
        var user1Contact = await CreateTestContactAsync(token1, name: "ユーザー1の連絡先");

        // Act - User 2 tries to access User 1's contact
        SetAuthorizationHeader(token2);
        var response = await Client.GetAsync($"/api/contacts/{user1Contact.Id}");

        // Assert - Should return NotFound (not Forbidden, to avoid information disclosure)
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContact_OtherUsersContact_ReturnsNotFound()
    {
        // Arrange - Create two users
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        // User 1 creates a contact
        var user1Contact = await CreateTestContactAsync(token1, name: "ユーザー1の連絡先");

        // Act - User 2 tries to update User 1's contact
        SetAuthorizationHeader(token2);
        var updateRequest = new UpdateContactRequest
        {
            Name = "不正な更新"
        };
        var response = await Client.PutAsJsonAsync($"/api/contacts/{user1Contact.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Verify the contact was not modified
        SetAuthorizationHeader(token1);
        var getResponse = await Client.GetAsync($"/api/contacts/{user1Contact.Id}");
        var contact = await getResponse.Content.ReadFromJsonAsync<ContactDto>();
        Assert.NotNull(contact);
        Assert.Equal("ユーザー1の連絡先", contact.Name);
    }

    [Fact]
    public async Task DeleteContact_OtherUsersContact_ReturnsNotFound()
    {
        // Arrange - Create two users
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        // User 1 creates a contact
        var user1Contact = await CreateTestContactAsync(token1, name: "ユーザー1の連絡先");

        // Act - User 2 tries to delete User 1's contact
        SetAuthorizationHeader(token2);
        var response = await Client.DeleteAsync($"/api/contacts/{user1Contact.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Verify the contact still exists
        SetAuthorizationHeader(token1);
        var getResponse = await Client.GetAsync($"/api/contacts/{user1Contact.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateContact_DuplicateName_OnlyChecksOwnContacts()
    {
        // Arrange - Create two users
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        var sameName = "共通の名前";

        // User 1 creates a contact
        await CreateTestContactAsync(token1, name: sameName);

        // Act - User 2 creates a contact with the same name (should succeed)
        SetAuthorizationHeader(token2);
        var request = new CreateContactRequest
        {
            Name = sameName
        };
        var response = await Client.PostAsJsonAsync("/api/contacts", request);

        // Assert - Should succeed because duplicate check is per-user
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetContacts_AfterUserSwitch_ReturnsCorrectContacts()
    {
        // Arrange - Create two users with contacts
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        await CreateTestContactAsync(token1, name: "ユーザー1の連絡先");
        await CreateTestContactAsync(token2, name: "ユーザー2の連絡先");

        // Act - Get contacts as User 1
        SetAuthorizationHeader(token1);
        var response1 = await Client.GetAsync("/api/contacts");
        var result1 = await response1.Content.ReadFromJsonAsync<ContactListResponse>();

        // Switch to User 2
        SetAuthorizationHeader(token2);
        var response2 = await Client.GetAsync("/api/contacts");
        var result2 = await response2.Content.ReadFromJsonAsync<ContactListResponse>();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Single(result1.Contacts);
        Assert.Single(result2.Contacts);
        Assert.Equal("ユーザー1の連絡先", result1.Contacts[0].Name);
        Assert.Equal("ユーザー2の連絡先", result2.Contacts[0].Name);
    }

    [Fact]
    public async Task ContactOperations_IsolatedBetweenUsers()
    {
        // Arrange - Create three users
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();
        var (token3, userId3, email3) = await RegisterAndLoginUserAsync();

        // Each user creates contacts
        await CreateTestContactAsync(token1, name: "User1-Contact1");
        await CreateTestContactAsync(token1, name: "User1-Contact2");
        await CreateTestContactAsync(token2, name: "User2-Contact1");
        await CreateTestContactAsync(token3, name: "User3-Contact1");
        await CreateTestContactAsync(token3, name: "User3-Contact2");
        await CreateTestContactAsync(token3, name: "User3-Contact3");

        // Act & Assert - Verify each user sees only their contacts
        SetAuthorizationHeader(token1);
        var response1 = await Client.GetAsync("/api/contacts");
        var result1 = await response1.Content.ReadFromJsonAsync<ContactListResponse>();
        Assert.Equal(2, result1!.Contacts.Count);
        Assert.All(result1.Contacts, c => Assert.StartsWith("User1", c.Name));

        SetAuthorizationHeader(token2);
        var response2 = await Client.GetAsync("/api/contacts");
        var result2 = await response2.Content.ReadFromJsonAsync<ContactListResponse>();
        Assert.Single(result2!.Contacts);
        Assert.StartsWith("User2", result2.Contacts[0].Name);

        SetAuthorizationHeader(token3);
        var response3 = await Client.GetAsync("/api/contacts");
        var result3 = await response3.Content.ReadFromJsonAsync<ContactListResponse>();
        Assert.Equal(3, result3!.Contacts.Count);
        Assert.All(result3.Contacts, c => Assert.StartsWith("User3", c.Name));
    }

    [Fact]
    public async Task AccessControl_PreventsInformationDisclosure()
    {
        // Arrange - Create two users
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        // User 1 creates a contact
        var user1Contact = await CreateTestContactAsync(token1, name: "機密連絡先");

        // Act - User 2 tries various operations on User 1's contact
        SetAuthorizationHeader(token2);

        var getResponse = await Client.GetAsync($"/api/contacts/{user1Contact.Id}");
        var updateResponse = await Client.PutAsJsonAsync($"/api/contacts/{user1Contact.Id}", 
            new UpdateContactRequest { Name = "Test" });
        var deleteResponse = await Client.DeleteAsync($"/api/contacts/{user1Contact.Id}");

        // Assert - All should return NotFound (not Forbidden) to prevent information disclosure
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task AccessControl_EnforcedAtDatabaseLevel()
    {
        // Arrange - Create two users
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        // User 1 creates contacts
        var contact1 = await CreateTestContactAsync(token1, name: "Contact1");
        var contact2 = await CreateTestContactAsync(token1, name: "Contact2");

        // User 2 creates contacts
        var contact3 = await CreateTestContactAsync(token2, name: "Contact3");

        // Act - Verify database-level isolation
        using var dbContext = GetDbContext();
        
        var user1Contacts = dbContext.Contacts
            .Where(c => c.UserId == userId1)
            .ToList();
        
        var user2Contacts = dbContext.Contacts
            .Where(c => c.UserId == userId2)
            .ToList();

        // Assert
        Assert.Equal(2, user1Contacts.Count);
        Assert.Single(user2Contacts);
        Assert.All(user1Contacts, c => Assert.Equal(userId1, c.UserId));
        Assert.All(user2Contacts, c => Assert.Equal(userId2, c.UserId));
    }

    [Fact]
    public async Task AccessControl_WorksWithPagination()
    {
        // Arrange - Create two users
        var (token1, userId1, email1) = await RegisterAndLoginUserAsync();
        var (token2, userId2, email2) = await RegisterAndLoginUserAsync();

        // User 1 creates 10 contacts
        for (int i = 1; i <= 10; i++)
        {
            await CreateTestContactAsync(token1, name: $"User1-Contact{i:D2}");
        }

        // User 2 creates 5 contacts
        for (int i = 1; i <= 5; i++)
        {
            await CreateTestContactAsync(token2, name: $"User2-Contact{i:D2}");
        }

        // Act - User 1 gets paginated results
        SetAuthorizationHeader(token1);
        var response1 = await Client.GetAsync("/api/contacts?page=1&pageSize=5");
        var result1 = await response1.Content.ReadFromJsonAsync<ContactListResponse>();

        // User 2 gets paginated results
        SetAuthorizationHeader(token2);
        var response2 = await Client.GetAsync("/api/contacts?page=1&pageSize=5");
        var result2 = await response2.Content.ReadFromJsonAsync<ContactListResponse>();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(5, result1.Contacts.Count);
        Assert.Equal(10, result1.Pagination.TotalCount);
        Assert.Equal(5, result2.Contacts.Count);
        Assert.Equal(5, result2.Pagination.TotalCount);
        Assert.All(result1.Contacts, c => Assert.StartsWith("User1", c.Name));
        Assert.All(result2.Contacts, c => Assert.StartsWith("User2", c.Name));
    }
}
