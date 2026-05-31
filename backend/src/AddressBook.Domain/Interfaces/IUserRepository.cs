using AddressBook.Domain.Entities;

namespace AddressBook.Domain.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
}
