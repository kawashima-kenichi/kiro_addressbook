using AddressBook.Application.DTOs;
using AddressBook.Application.Validators;
using FluentValidation.TestHelper;

namespace AddressBook.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_ShouldNotHaveErrors()
    {
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password1!"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region Email Validation

    [Fact]
    public void EmptyEmail_ShouldHaveError()
    {
        var request = new RegisterRequest { Email = "", Password = "Password1!" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("メールアドレスは必須です");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("invalid.com")]
    public void InvalidEmailFormat_ShouldHaveError(string email)
    {
        var request = new RegisterRequest { Email = email, Password = "Password1!" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("有効なメールアドレスを入力してください");
    }

    [Fact]
    public void EmailExceeding255Characters_ShouldHaveError()
    {
        var longEmail = new string('a', 244) + "@example.com"; // 256 chars
        var request = new RegisterRequest { Email = longEmail, Password = "Password1!" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("メールアドレスは255文字以下で入力してください");
    }

    [Fact]
    public void EmailExactly255Characters_ShouldNotHaveError()
    {
        var email = new string('a', 243) + "@example.com"; // 255 chars
        var request = new RegisterRequest { Email = email, Password = "Password1!" };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Validation

    [Fact]
    public void EmptyPassword_ShouldHaveError()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("パスワードは必須です");
    }

    [Fact]
    public void PasswordLessThan8Characters_ShouldHaveError()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Pass1!" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("パスワードは8文字以上で入力してください");
    }

    [Fact]
    public void PasswordWithoutUppercase_ShouldHaveError()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "password1!" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("パスワードは大文字、小文字、数字、特殊文字を含む必要があります");
    }

    [Fact]
    public void PasswordWithoutLowercase_ShouldHaveError()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "PASSWORD1!" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("パスワードは大文字、小文字、数字、特殊文字を含む必要があります");
    }

    [Fact]
    public void PasswordWithoutDigit_ShouldHaveError()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Password!" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("パスワードは大文字、小文字、数字、特殊文字を含む必要があります");
    }

    [Fact]
    public void PasswordWithoutSpecialCharacter_ShouldHaveError()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Password1a" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("パスワードは大文字、小文字、数字、特殊文字を含む必要があります");
    }

    [Theory]
    [InlineData("Password1!")]
    [InlineData("MyP@ssw0rd")]
    [InlineData("Str0ng!Pass")]
    [InlineData("Ab1!defg")]
    public void ValidPassword_ShouldNotHaveError(string password)
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = password };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void PasswordExactly8Characters_ShouldNotHaveError()
    {
        var request = new RegisterRequest { Email = "test@example.com", Password = "Ab1!defg" };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}
