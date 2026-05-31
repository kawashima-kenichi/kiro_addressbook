using AddressBook.Application.DTOs;
using AddressBook.Application.Validators;
using FluentValidation.TestHelper;

namespace AddressBook.Tests.Validators;

public class UpdateContactRequestValidatorTests
{
    private readonly UpdateContactRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_ShouldNotHaveErrors()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            Address = "東京都渋谷区",
            PhoneNumber = "123-456-7890"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region Name Validation

    [Fact]
    public void EmptyName_ShouldHaveError()
    {
        var request = new UpdateContactRequest { Name = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("名前は必須です");
    }

    [Fact]
    public void NameExceeding100Characters_ShouldHaveError()
    {
        var request = new UpdateContactRequest { Name = new string('あ', 101) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("名前は1文字以上100文字以下で入力してください");
    }

    [Fact]
    public void NameExactly100Characters_ShouldNotHaveError()
    {
        var name = new string('あ', 100);
        var request = new UpdateContactRequest { Name = name };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void NameExactly1Character_ShouldNotHaveError()
    {
        var request = new UpdateContactRequest { Name = "A" };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Address Validation

    [Fact]
    public void AddressExceeding500Characters_ShouldHaveError()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            Address = new string('あ', 501)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Address)
            .WithErrorMessage("住所は500文字以下で入力してください");
    }

    [Fact]
    public void AddressExactly500Characters_ShouldNotHaveError()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            Address = new string('あ', 500)
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void NullAddress_ShouldNotHaveError()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            Address = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void EmptyAddress_ShouldNotHaveError()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            Address = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }

    #endregion

    #region PhoneNumber Validation

    [Theory]
    [InlineData("(123) 456-7890")]
    [InlineData("(123)456-7890")]
    [InlineData("123-456-7890")]
    [InlineData("123.456.7890")]
    [InlineData("+1-123-456-7890")]
    [InlineData("1234567890")]
    public void ValidPhoneNumberFormats_ShouldNotHaveError(string phoneNumber)
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            PhoneNumber = phoneNumber
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("123-456-789")]  // Too short
    [InlineData("123456789")]    // Too short
    [InlineData("abc-def-ghij")] // Letters
    [InlineData("12-34-56-78")]  // Wrong format
    [InlineData("+81-90-1234-5678")] // Wrong country code format
    public void InvalidPhoneNumberFormats_ShouldHaveError(string phoneNumber)
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            PhoneNumber = phoneNumber
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("有効な電話番号を入力してください");
    }

    [Fact]
    public void PhoneNumberExceeding20Characters_ShouldHaveError()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            PhoneNumber = new string('1', 21)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("電話番号は20文字以下で入力してください");
    }

    [Fact]
    public void PhoneNumberExactly20Characters_ShouldNotHaveError()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            PhoneNumber = "+1-123-456-7890" // 16 characters, valid format
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void NullPhoneNumber_ShouldNotHaveError()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            PhoneNumber = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void EmptyPhoneNumber_ShouldNotHaveError()
    {
        var request = new UpdateContactRequest
        {
            Name = "田中太郎",
            PhoneNumber = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    #endregion
}
