using AddressBook.Application.DTOs;
using AddressBook.Application.Interfaces;
using AddressBook.Domain.Entities;
using AddressBook.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AddressBook.Tests.Services;

public class AuthServiceTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var userStore = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            userStore, null, null, null, null, null, null, null, null);

        var contextAccessor = Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            _userManager, contextAccessor, claimsFactory, null, null, null, null);

        _jwtService = Substitute.For<IJwtService>();
        _logger = Substitute.For<ILogger<AuthService>>();

        _authService = new AuthService(_userManager, _signInManager, _jwtService, _logger);
    }

    #region Login Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var request = new LoginRequest { Email = "test@example.com", Password = "Password1!" };

        _userManager.FindByEmailAsync(request.Email).Returns(user);
        _userManager.IsLockedOutAsync(user).Returns(false);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true)
            .Returns(SignInResult.Success);
        _jwtService.GenerateToken(user).Returns("test-jwt-token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-jwt-token", result.Token);
        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal(user.Email, result.User.Email);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ThrowsAuthenticationException()
    {
        // Arrange
        var request = new LoginRequest { Email = "nonexistent@example.com", Password = "Password1!" };
        _userManager.FindByEmailAsync(request.Email).ReturnsNull();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AuthenticationException>(
            () => _authService.LoginAsync(request));
        Assert.Equal("ユーザー名またはパスワードが無効です", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsAuthenticationException()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword!" };

        _userManager.FindByEmailAsync(request.Email).Returns(user);
        _userManager.IsLockedOutAsync(user).Returns(false);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true)
            .Returns(SignInResult.Failed);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AuthenticationException>(
            () => _authService.LoginAsync(request));
        Assert.Equal("ユーザー名またはパスワードが無効です", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_LockedAccount_ThrowsAccountLockedException()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var request = new LoginRequest { Email = "test@example.com", Password = "Password1!" };

        _userManager.FindByEmailAsync(request.Email).Returns(user);
        _userManager.IsLockedOutAsync(user).Returns(true);
        _userManager.GetLockoutEndDateAsync(user)
            .Returns(DateTimeOffset.UtcNow.AddMinutes(25));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AccountLockedException>(
            () => _authService.LoginAsync(request));
        Assert.Equal("アカウントがロックされています。30分後に再試行してください。", ex.Message);
        Assert.True(ex.RetryAfterSeconds > 0);
    }

    [Fact]
    public async Task LoginAsync_PasswordCheckCausesLockout_ThrowsAccountLockedException()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword!" };

        _userManager.FindByEmailAsync(request.Email).Returns(user);
        _userManager.IsLockedOutAsync(user).Returns(false);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true)
            .Returns(SignInResult.LockedOut);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AccountLockedException>(
            () => _authService.LoginAsync(request));
        Assert.Equal("アカウントがロックされています。30分後に再試行してください。", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_Success_ResetsAccessFailedCount()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com"
        };
        var request = new LoginRequest { Email = "test@example.com", Password = "Password1!" };

        _userManager.FindByEmailAsync(request.Email).Returns(user);
        _userManager.IsLockedOutAsync(user).Returns(false);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true)
            .Returns(SignInResult.Success);
        _jwtService.GenerateToken(user).Returns("test-jwt-token");

        // Act
        await _authService.LoginAsync(request);

        // Assert
        await _userManager.Received(1).ResetAccessFailedCountAsync(user);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_ValidData_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@example.com", Password = "Password1!" };
        _userManager.FindByEmailAsync(request.Email).ReturnsNull();
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), request.Password)
            .Returns(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("アカウントが正常に作成されました", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        // Arrange
        var existingUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com"
        };
        var request = new RegisterRequest { Email = "existing@example.com", Password = "Password1!" };
        _userManager.FindByEmailAsync(request.Email).Returns(existingUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DuplicateEmailException>(
            () => _authService.RegisterAsync(request));
        Assert.Equal("このメールアドレスは既に使用されています", ex.Message);
    }

    [Fact]
    public async Task RegisterAsync_IdentityFailure_ThrowsRegistrationException()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@example.com", Password = "weak" };
        _userManager.FindByEmailAsync(request.Email).ReturnsNull();
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), request.Password)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RegistrationException>(
            () => _authService.RegisterAsync(request));
        Assert.Equal("アカウントを作成できません。もう一度お試しください。", ex.Message);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task LogoutAsync_ValidUser_ReturnsSuccessResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _authService.LogoutAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("ログアウトしました", result.Message);
    }

    #endregion
}
