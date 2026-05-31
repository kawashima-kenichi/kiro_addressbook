using AddressBook.Application.DTOs;

namespace AddressBook.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<LogoutResponse> LogoutAsync(Guid userId);
}
