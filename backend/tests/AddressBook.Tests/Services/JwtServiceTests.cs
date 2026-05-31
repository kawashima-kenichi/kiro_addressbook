using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AddressBook.Domain.Entities;
using AddressBook.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace AddressBook.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public JwtServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JwtSettings:Key", "TestSecretKeyThatIsAtLeast32CharactersLongForHmacSha256!" },
            { "JwtSettings:Issuer", "AddressBook.API.Test" },
            { "JwtSettings:Audience", "AddressBook.Client.Test" },
            { "JwtSettings:ExpirationHours", "24" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _jwtService = new JwtService(_configuration);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        Assert.Equal(userId.ToString(), jwtToken.Subject);
        Assert.Equal("test@example.com", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("AddressBook.API.Test", jwtToken.Issuer);
        Assert.Contains("AddressBook.Client.Test", jwtToken.Audiences);
    }

    [Fact]
    public void GenerateToken_ExpiresIn24Hours()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddHours(24);
        Assert.True(jwtToken.ValidTo <= expectedExpiry.AddMinutes(1));
        Assert.True(jwtToken.ValidTo >= expectedExpiry.AddMinutes(-1));
    }

    [Fact]
    public void ValidateToken_ValidToken_ReturnsTrue()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var isValid = _jwtService.ValidateToken(token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsFalse()
    {
        // Act
        var isValid = _jwtService.ValidateToken("invalid.token.here");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GetUserIdFromToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var token = _jwtService.GenerateToken(user);

        // Act
        var extractedUserId = _jwtService.GetUserIdFromToken(token);

        // Assert
        Assert.NotNull(extractedUserId);
        Assert.Equal(userId, extractedUserId.Value);
    }

    [Fact]
    public void GetUserIdFromToken_InvalidToken_ReturnsNull()
    {
        // Act
        var userId = _jwtService.GetUserIdFromToken("invalid.token.here");

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void GenerateToken_EachTokenHasUniqueJti()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        // Act
        var token1 = _jwtService.GenerateToken(user);
        var token2 = _jwtService.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jti1 = handler.ReadJwtToken(token1).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = handler.ReadJwtToken(token2).Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        // Assert
        Assert.NotEqual(jti1, jti2);
    }
}
