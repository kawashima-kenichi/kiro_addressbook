using AddressBook.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AddressBook.Infrastructure.Services;

/// <summary>
/// Custom password hasher using BCrypt with a minimum of 12 rounds.
/// Satisfies requirement 9.3: Passwords hashed with bcrypt (minimum 12 rounds), no plaintext storage.
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private const int WorkFactor = 12;

    public string HashPassword(ApplicationUser user, string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user, string hashedPassword, string providedPassword)
    {
        var isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);

        if (!isValid)
        {
            return PasswordVerificationResult.Failed;
        }

        // Check if the hash needs to be rehashed (e.g., if work factor was increased)
        var logRounds = BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor)
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Success;

        return logRounds;
    }
}
