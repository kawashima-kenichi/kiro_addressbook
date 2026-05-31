using AddressBook.Application.DTOs;
using AddressBook.Application.Interfaces;
using AddressBook.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AddressBook.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            throw new AuthenticationException("ユーザー名またはパスワードが無効です");
        }

        // Check if account is locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var retryAfterSeconds = lockoutEnd.HasValue
                ? (int)Math.Ceiling((lockoutEnd.Value - DateTimeOffset.UtcNow).TotalSeconds)
                : 1800;

            _logger.LogWarning("Login attempt on locked account: {Email}", request.Email);
            throw new AccountLockedException(
                "アカウントがロックされています。30分後に再試行してください。",
                retryAfterSeconds);
        }

        // Attempt sign-in with lockout enabled
        var result = await _signInManager.CheckPasswordSignInAsync(
            user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Account locked after failed attempts: {Email}", request.Email);
            throw new AccountLockedException(
                "アカウントがロックされています。30分後に再試行してください。",
                1800);
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for: {Email}", request.Email);
            throw new AuthenticationException("ユーザー名またはパスワードが無効です");
        }

        // Reset failed login attempts on successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        // Update last login timestamp
        user.LastLogin = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate JWT token (24-hour expiration configured in JwtService)
        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        _logger.LogInformation("User logged in successfully: {Email}", request.Email);

        return new LoginResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty
            },
            ExpiresAt = expiresAt
        };
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new DuplicateEmailException("このメールアドレスは既に使用されています");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, errors);
            throw new RegistrationException($"アカウントを作成できません。もう一度お試しください。");
        }

        _logger.LogInformation("User registered successfully: {Email}", request.Email);

        return new RegisterResponse
        {
            Success = true,
            Message = "アカウントが正常に作成されました"
        };
    }

    public Task<LogoutResponse> LogoutAsync(Guid userId)
    {
        // JWT is stateless - logout is handled client-side by discarding the token.
        // Server-side we just acknowledge the logout request.
        _logger.LogInformation("User logged out: {UserId}", userId);

        return Task.FromResult(new LogoutResponse
        {
            Success = true,
            Message = "ログアウトしました"
        });
    }
}

// Custom exception types for auth operations
public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message) { }
}

public class AccountLockedException : Exception
{
    public int RetryAfterSeconds { get; }

    public AccountLockedException(string message, int retryAfterSeconds)
        : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string message) : base(message) { }
}

public class RegistrationException : Exception
{
    public RegistrationException(string message) : base(message) { }
}
