using AddressBook.Domain.Entities;

namespace AddressBook.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
