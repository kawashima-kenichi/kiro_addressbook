using AddressBook.Application.DTOs;
using AddressBook.Application.Validators;
using FluentValidation.TestHelper;

namespace AddressBook.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_ShouldNotHaveErrors()
    {
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "anypassword"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyEmail_ShouldHaveError()
    {
        var request = new LoginRequest { Email = "", Password = "password" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("メールアドレスは必須です");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    public void InvalidEmailFormat_ShouldHaveError(string email)
    {
        var request = new LoginRequest { Email = email, Password = "password" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("有効なメールアドレスを入力してください");
    }

    [Fact]
    public void EmptyPassword_ShouldHaveError()
    {
        var request = new LoginRequest { Email = "test@example.com", Password = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("パスワードは必須です");
    }
}
