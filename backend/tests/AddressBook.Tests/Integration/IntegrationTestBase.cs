using System.Net.Http.Headers;
using System.Net.Http.Json;
using AddressBook.Application.DTOs;
using AddressBook.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AddressBook.Tests.Integration;

/// <summary>
/// 統合テストの基底クラス - 共通のヘルパーメソッドを提供
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>
    /// テストユーザーを登録し、JWTトークンを取得
    /// </summary>
    protected async Task<(string Token, Guid UserId, string Email)> RegisterAndLoginUserAsync(
        string? email = null,
        string? password = null)
    {
        email ??= $"test-{Guid.NewGuid()}@example.com";
        password ??= "Test@1234";

        // Register user
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        // Login to get token
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);

        return (loginResult.Token, loginResult.User.Id, email);
    }

    /// <summary>
    /// 認証ヘッダーを設定
    /// </summary>
    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// 認証ヘッダーをクリア
    /// </summary>
    protected void ClearAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// データベースコンテキストを取得（データベース操作用）
    /// </summary>
    protected ApplicationDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// テスト用の連絡先を作成
    /// </summary>
    protected async Task<ContactDto> CreateTestContactAsync(
        string token,
        string? name = null,
        string? address = null,
        string? phoneNumber = null)
    {
        SetAuthorizationHeader(token);

        var request = new CreateContactRequest
        {
            Name = name ?? $"Test Contact {Guid.NewGuid()}",
            Address = address,
            PhoneNumber = phoneNumber
        };

        var response = await Client.PostAsJsonAsync("/api/contacts", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ContactSuccessResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Contact);

        return result.Contact;
    }
}
