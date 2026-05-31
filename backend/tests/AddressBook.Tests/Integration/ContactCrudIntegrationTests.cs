using System.Net;
using System.Net.Http.Json;
using AddressBook.Application.DTOs;

namespace AddressBook.Tests.Integration;

/// <summary>
/// 連絡先CRUD操作統合テスト
/// 要件: 2.1 (連絡先追加), 3.1 (連絡先表示), 4.2 (連絡先編集), 5.2 (連絡先削除)
/// </summary>
public class ContactCrudIntegrationTests : IntegrationTestBase
{
    public ContactCrudIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    #region Create Tests

    [Fact]
    public async Task CreateContact_ValidData_ReturnsCreatedContact()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var request = new CreateContactRequest
        {
            Name = "田中太郎",
            Address = "東京都渋谷区",
            PhoneNumber = "03-1234-5678"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactSuccessResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Contact);
        Assert.Equal("田中太郎", result.Contact.Name);
        Assert.Equal("東京都渋谷区", result.Contact.Address);
        Assert.Equal("03-1234-5678", result.Contact.PhoneNumber);
        Assert.Contains("正常に追加されました", result.Message);
    }

    [Fact]
    public async Task CreateContact_MinimalData_ReturnsCreatedContact()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var request = new CreateContactRequest
        {
            Name = "山田花子"
            // Address and PhoneNumber are optional
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactSuccessResponse>();
        Assert.NotNull(result);
        Assert.Equal("山田花子", result.Contact!.Name);
        Assert.Null(result.Contact.Address);
        Assert.Null(result.Contact.PhoneNumber);
    }

    [Fact]
    public async Task CreateContact_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var name = "重複テスト";
        await CreateTestContactAsync(token, name: name);

        var request = new CreateContactRequest
        {
            Name = name
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactErrorResponse>();
        Assert.NotNull(result);
        Assert.Contains("既に存在します", result.Message);
    }

    [Fact]
    public async Task CreateContact_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var request = new CreateContactRequest
        {
            Name = ""
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateContact_NameTooLong_ReturnsBadRequest()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var request = new CreateContactRequest
        {
            Name = new string('あ', 101) // 101 characters
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateContact_InvalidPhoneNumber_ReturnsBadRequest()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var request = new CreateContactRequest
        {
            Name = "電話番号テスト",
            PhoneNumber = "invalid-phone"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateContact_ValidPhoneNumberFormats_ReturnsSuccess()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var validFormats = new[]
        {
            "03-1234-5678",
            "(03) 1234-5678",
            "03.1234.5678",
            "+81-3-1234-5678",
            "0312345678"
        };

        // Act & Assert
        foreach (var format in validFormats)
        {
            var request = new CreateContactRequest
            {
                Name = $"電話テスト {format}",
                PhoneNumber = format
            };

            var response = await Client.PostAsJsonAsync("/api/contacts", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }

    [Fact]
    public async Task CreateContact_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange - No authentication
        var request = new CreateContactRequest
        {
            Name = "認証なしテスト"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Read Tests

    [Fact]
    public async Task GetContacts_EmptyList_ReturnsEmptyArray()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        // Act
        var response = await Client.GetAsync("/api/contacts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactListResponse>();
        Assert.NotNull(result);
        Assert.Empty(result.Contacts);
        Assert.Equal(0, result.Pagination.TotalCount);
    }

    [Fact]
    public async Task GetContacts_WithContacts_ReturnsSortedList()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        // Create contacts in random order
        await CreateTestContactAsync(token, name: "佐藤");
        await CreateTestContactAsync(token, name: "田中");
        await CreateTestContactAsync(token, name: "鈴木");

        // Act
        var response = await Client.GetAsync("/api/contacts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactListResponse>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Contacts.Count);
        Assert.Equal(3, result.Pagination.TotalCount);
        
        // Verify alphabetical sorting
        Assert.Equal("佐藤", result.Contacts[0].Name);
        Assert.Equal("田中", result.Contacts[1].Name);
        Assert.Equal("鈴木", result.Contacts[2].Name);
    }

    [Fact]
    public async Task GetContacts_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        // Create 5 contacts
        for (int i = 1; i <= 5; i++)
        {
            await CreateTestContactAsync(token, name: $"連絡先{i:D2}");
        }

        // Act - Get page 2 with 2 items per page
        var response = await Client.GetAsync("/api/contacts?page=2&pageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactListResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Contacts.Count);
        Assert.Equal(5, result.Pagination.TotalCount);
        Assert.Equal(2, result.Pagination.CurrentPage);
        Assert.Equal(2, result.Pagination.PageSize);
        Assert.Equal(3, result.Pagination.TotalPages);
        Assert.True(result.Pagination.HasPrevious);
        Assert.True(result.Pagination.HasNext);
    }

    [Fact]
    public async Task GetContactById_ExistingContact_ReturnsContact()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        var contact = await CreateTestContactAsync(token, name: "取得テスト");

        // Act
        var response = await Client.GetAsync($"/api/contacts/{contact.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactDto>();
        Assert.NotNull(result);
        Assert.Equal(contact.Id, result.Id);
        Assert.Equal("取得テスト", result.Name);
    }

    [Fact]
    public async Task GetContactById_NonExistentContact_ReturnsNotFound()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/contacts/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateContact_ValidData_ReturnsUpdatedContact()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        var contact = await CreateTestContactAsync(token, name: "更新前");

        var updateRequest = new UpdateContactRequest
        {
            Name = "更新後",
            Address = "新しい住所",
            PhoneNumber = "090-1234-5678"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/contacts/{contact.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactSuccessResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("更新後", result.Contact!.Name);
        Assert.Equal("新しい住所", result.Contact.Address);
        Assert.Equal("090-1234-5678", result.Contact.PhoneNumber);
        Assert.Contains("正常に更新されました", result.Message);
    }

    [Fact]
    public async Task UpdateContact_NonExistentContact_ReturnsNotFound()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateContactRequest
        {
            Name = "存在しない"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/contacts/{nonExistentId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContact_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        var contact = await CreateTestContactAsync(token, name: "検証テスト");

        var updateRequest = new UpdateContactRequest
        {
            Name = "" // Empty name is invalid
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/contacts/{contact.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContact_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        await CreateTestContactAsync(token, name: "既存の名前");
        var contact = await CreateTestContactAsync(token, name: "更新対象");

        var updateRequest = new UpdateContactRequest
        {
            Name = "既存の名前" // Duplicate
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/contacts/{contact.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteContact_ExistingContact_ReturnsSuccess()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        var contact = await CreateTestContactAsync(token, name: "削除テスト");

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contact.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactSuccessResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Contains("正常に削除されました", result.Message);

        // Verify contact is actually deleted
        var getResponse = await Client.GetAsync($"/api/contacts/{contact.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteContact_NonExistentContact_ReturnsNotFound()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(token);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteContact_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        var contact = await CreateTestContactAsync(token, name: "認証テスト");

        // Clear authentication
        ClearAuthorizationHeader();

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contact.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Data Persistence Tests

    [Fact]
    public async Task ContactData_PersistsAcrossRequests()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        var contact = await CreateTestContactAsync(
            token,
            name: "永続化テスト",
            address: "テスト住所",
            phoneNumber: "03-9999-9999"
        );

        // Act - Retrieve the contact in a separate request
        var response = await Client.GetAsync($"/api/contacts/{contact.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactDto>();
        Assert.NotNull(result);
        Assert.Equal("永続化テスト", result.Name);
        Assert.Equal("テスト住所", result.Address);
        Assert.Equal("03-9999-9999", result.PhoneNumber);
        Assert.NotEqual(default, result.CreatedAt);
        Assert.NotEqual(default, result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateContact_UpdatesTimestamp()
    {
        // Arrange
        var (token, userId, email) = await RegisterAndLoginUserAsync();
        var contact = await CreateTestContactAsync(token, name: "タイムスタンプテスト");

        var originalUpdatedAt = contact.UpdatedAt;
        await Task.Delay(1000); // Wait 1 second

        var updateRequest = new UpdateContactRequest
        {
            Name = "タイムスタンプテスト更新"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/contacts/{contact.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ContactSuccessResponse>();
        Assert.NotNull(result);
        Assert.True(result.Contact!.UpdatedAt > originalUpdatedAt);
    }

    #endregion
}
