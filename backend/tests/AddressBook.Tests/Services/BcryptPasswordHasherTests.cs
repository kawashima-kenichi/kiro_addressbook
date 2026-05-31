using AddressBook.Domain.Entities;
using AddressBook.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;

namespace AddressBook.Tests.Services;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher;
    private readonly ApplicationUser _user;

    public BcryptPasswordHasherTests()
    {
        _hasher = new BcryptPasswordHasher();
        _user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
    }

    [Fact]
    public void HashPassword_ReturnsNonEmptyHash()
    {
        // Act
        var hash = _hasher.HashPassword(_user, "TestPassword123!");

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void HashPassword_DoesNotReturnPlaintext()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _hasher.HashPassword(_user, password);

        // Assert
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void HashPassword_UsesBcryptFormat()
    {
        // Act
        var hash = _hasher.HashPassword(_user, "TestPassword123!");

        // Assert - bcrypt hashes start with $2a$, $2b$, or $2y$
        Assert.Matches(@"^\$2[aby]\$\d{2}\$", hash);
    }

    [Fact]
    public void HashPassword_UsesAtLeast12Rounds()
    {
        // Act
        var hash = _hasher.HashPassword(_user, "TestPassword123!");

        // Assert - extract work factor from hash (format: $2a$12$...)
        var parts = hash.Split('$');
        var workFactor = int.Parse(parts[2]);
        Assert.True(workFactor >= 12, $"Expected work factor >= 12, but got {workFactor}");
    }

    [Fact]
    public void HashPassword_SamePasswordProducesDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _hasher.HashPassword(_user, password);
        var hash2 = _hasher.HashPassword(_user, password);

        // Assert - bcrypt uses random salt, so hashes should differ
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyHashedPassword_CorrectPassword_ReturnsSuccess()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _hasher.HashPassword(_user, password);

        // Act
        var result = _hasher.VerifyHashedPassword(_user, hash, password);

        // Assert
        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void VerifyHashedPassword_WrongPassword_ReturnsFailed()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _hasher.HashPassword(_user, password);

        // Act
        var result = _hasher.VerifyHashedPassword(_user, hash, "WrongPassword456!");

        // Assert
        Assert.Equal(PasswordVerificationResult.Failed, result);
    }

    [Fact]
    public void VerifyHashedPassword_OlderWorkFactor_ReturnsSuccessRehashNeeded()
    {
        // Arrange - create a hash with a lower work factor (10 rounds)
        var password = "TestPassword123!";
        var lowerWorkFactorHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);

        // Act
        var result = _hasher.VerifyHashedPassword(_user, lowerWorkFactorHash, password);

        // Assert - should succeed but indicate rehash is needed
        Assert.Equal(PasswordVerificationResult.SuccessRehashNeeded, result);
    }
}
