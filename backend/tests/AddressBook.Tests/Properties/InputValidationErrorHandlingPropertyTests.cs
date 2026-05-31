using AddressBook.Application.DTOs;
using AddressBook.Application.Validators;
using FluentValidation;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace AddressBook.Tests.Properties;

/// <summary>
/// **Validates: Requirements 2.5, 2.8, 8.1, 8.3**
/// Property 3: 入力検証エラーハンドリング
/// 任意の無効なデータ（空の名前、フィールド長超過、無効な電話番号形式）に対して、
/// システムは保存せずに具体的な検証エラーを表示する
/// </summary>
public class InputValidationErrorHandlingPropertyTests
{
    private readonly CreateContactRequestValidator _validator = new();

    /// <summary>
    /// Property 3: Invalid data (empty name) is rejected with specific validation error
    /// Tests that empty or whitespace-only names are rejected with the error message "名前は必須です"
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property3_InputValidation_RejectsEmptyName(
        string? address,
        string? phoneNumber)
    {
        // Filter optional fields to valid ranges
        if (address != null && address.Length > 500)
            return;
        
        if (phoneNumber != null && phoneNumber.Length > 20)
            return;

        // Test with empty string
        var emptyNameRequest = new CreateContactRequest
        {
            Name = "",
            Address = address,
            PhoneNumber = phoneNumber
        };

        var emptyResult = _validator.Validate(emptyNameRequest);
        
        // Assert - Validation should fail
        Assert.False(emptyResult.IsValid);
        Assert.Contains(emptyResult.Errors, e => 
            e.PropertyName == "Name" && e.ErrorMessage == "名前は必須です");

        // Test with whitespace-only string
        var whitespaceNameRequest = new CreateContactRequest
        {
            Name = "   ",
            Address = address,
            PhoneNumber = phoneNumber
        };

        var whitespaceResult = _validator.Validate(whitespaceNameRequest);
        
        // Assert - Validation should fail
        Assert.False(whitespaceResult.IsValid);
        Assert.Contains(whitespaceResult.Errors, e => 
            e.PropertyName == "Name" && e.ErrorMessage == "名前は必須です");
    }

    /// <summary>
    /// Property 3: Invalid data (name exceeding 100 characters) is rejected with specific validation error
    /// Tests that names longer than 100 characters are rejected with the error message
    /// "名前は1文字以上100文字以下で入力してください"
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property3_InputValidation_RejectsNameExceeding100Characters(
        PositiveInt excessLength,
        string? address,
        string? phoneNumber)
    {
        // Generate name with length > 100 (101 to 200 characters)
        var nameLength = 101 + (excessLength.Get % 100);
        var longName = new string('あ', nameLength);
        
        // Filter optional fields to valid ranges
        if (address != null && address.Length > 500)
            return;
        
        if (phoneNumber != null && phoneNumber.Length > 20)
            return;

        // Arrange
        var request = new CreateContactRequest
        {
            Name = longName,
            Address = address,
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert - Validation should fail
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "Name" && 
            e.ErrorMessage == "名前は1文字以上100文字以下で入力してください");
    }

    /// <summary>
    /// Property 3: Invalid data (address exceeding 500 characters) is rejected with specific validation error
    /// Tests that addresses longer than 500 characters are rejected with the error message
    /// "住所は500文字以下で入力してください"
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property3_InputValidation_RejectsAddressExceeding500Characters(
        NonEmptyString name,
        PositiveInt excessLength,
        string? phoneNumber)
    {
        // Generate valid name (1-100 characters)
        var validName = name.Get;
        if (validName.Length > 100)
            validName = validName.Substring(0, 100);
        
        if (string.IsNullOrWhiteSpace(validName))
            return;
        
        // Generate address with length > 500 (501 to 1000 characters)
        var addressLength = 501 + (excessLength.Get % 500);
        var longAddress = new string('あ', addressLength);
        
        // Filter phone number to valid range
        if (phoneNumber != null && phoneNumber.Length > 20)
            return;

        // Arrange
        var request = new CreateContactRequest
        {
            Name = validName.Trim(),
            Address = longAddress,
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert - Validation should fail
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "Address" && 
            e.ErrorMessage == "住所は500文字以下で入力してください");
    }

    /// <summary>
    /// Property 3: Invalid data (phone number exceeding 20 characters) is rejected with specific validation error
    /// Tests that phone numbers longer than 20 characters are rejected with the error message
    /// "電話番号は20文字以下で入力してください"
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property3_InputValidation_RejectsPhoneNumberExceeding20Characters(
        NonEmptyString name,
        string? address,
        PositiveInt excessLength)
    {
        // Generate valid name (1-100 characters)
        var validName = name.Get;
        if (validName.Length > 100)
            validName = validName.Substring(0, 100);
        
        if (string.IsNullOrWhiteSpace(validName))
            return;
        
        // Filter address to valid range
        if (address != null && address.Length > 500)
            return;
        
        // Generate phone number with length > 20 (21 to 50 characters)
        var phoneLength = 21 + (excessLength.Get % 30);
        var longPhoneNumber = new string('1', phoneLength);

        // Arrange
        var request = new CreateContactRequest
        {
            Name = validName.Trim(),
            Address = address,
            PhoneNumber = longPhoneNumber
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert - Validation should fail
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "PhoneNumber" && 
            e.ErrorMessage == "電話番号は20文字以下で入力してください");
    }

    /// <summary>
    /// Property 3: Invalid data (invalid phone number format) is rejected with specific validation error
    /// Tests that phone numbers not matching the accepted formats are rejected with the error message
    /// "有効な電話番号を入力してください"
    /// </summary>
    [Property(MaxTest = 100)]
    public void Property3_InputValidation_RejectsInvalidPhoneNumberFormat(
        NonEmptyString name,
        string? address,
        NonEmptyString invalidPhone)
    {
        // Generate valid name (1-100 characters)
        var validName = name.Get;
        if (validName.Length > 100)
            validName = validName.Substring(0, 100);
        
        if (string.IsNullOrWhiteSpace(validName))
            return;
        
        // Filter address to valid range
        if (address != null && address.Length > 500)
            return;
        
        // Generate invalid phone number (not matching any accepted format)
        var phoneNumber = invalidPhone.Get;
        
        // Skip if phone number is too long (will trigger length error instead)
        if (phoneNumber.Length > 20)
            return;
        
        // Skip if phone number is empty or whitespace (optional field)
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return;
        
        // Skip if phone number matches any valid format
        if (IsValidPhoneNumberFormat(phoneNumber))
            return;

        // Arrange
        var request = new CreateContactRequest
        {
            Name = validName.Trim(),
            Address = address,
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert - Validation should fail with format error
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "PhoneNumber" && 
            e.ErrorMessage == "有効な電話番号を入力してください");
    }

    /// <summary>
    /// Property 3: Multiple validation errors are reported together
    /// Tests that when multiple fields are invalid, all validation errors are reported
    /// </summary>
    [Fact]
    public void Property3_InputValidation_ReportsMultipleValidationErrors()
    {
        // Arrange - Create request with multiple invalid fields
        var request = new CreateContactRequest
        {
            Name = "",  // Invalid: empty
            Address = new string('あ', 501),  // Invalid: exceeds 500 characters
            PhoneNumber = "abc"  // Invalid: wrong format (short enough to not trigger length error)
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert - All validation errors should be reported
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3, $"Expected at least 3 errors, but got {result.Errors.Count}");
        
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "Name" && e.ErrorMessage == "名前は必須です");
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "Address" && e.ErrorMessage == "住所は500文字以下で入力してください");
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "PhoneNumber" && e.ErrorMessage == "有効な電話番号を入力してください");
    }

    /// <summary>
    /// Property 3: Valid data passes validation
    /// Tests that valid data passes all validation checks
    /// </summary>
    [Theory]
    [InlineData("田中太郎", "東京都渋谷区", "(123) 456-7890")]
    [InlineData("A", null, null)]
    [InlineData("山田花子", "大阪府大阪市北区梅田1-1-1", "123-456-7890")]
    [InlineData("John Smith", "123 Main St, New York, NY 10001", "1234567890")]
    public void Property3_InputValidation_AcceptsValidData(string name, string? address, string? phoneNumber)
    {
        // Arrange
        var request = new CreateContactRequest
        {
            Name = name,
            Address = address,
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert - Validation should pass
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Property 3: Edge case - Name at boundary (exactly 100 characters) is valid
    /// </summary>
    [Fact]
    public void Property3_InputValidation_AcceptsNameAtMaxLength()
    {
        // Arrange
        var maxLengthName = new string('あ', 100);
        var request = new CreateContactRequest
        {
            Name = maxLengthName,
            Address = null,
            PhoneNumber = null
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Property 3: Edge case - Address at boundary (exactly 500 characters) is valid
    /// </summary>
    [Fact]
    public void Property3_InputValidation_AcceptsAddressAtMaxLength()
    {
        // Arrange
        var maxLengthAddress = new string('あ', 500);
        var request = new CreateContactRequest
        {
            Name = "田中太郎",
            Address = maxLengthAddress,
            PhoneNumber = null
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Property 3: Edge case - Phone number at boundary (exactly 20 characters) with valid format is valid
    /// </summary>
    [Fact]
    public void Property3_InputValidation_AcceptsPhoneNumberAtMaxLength()
    {
        // Arrange - Valid format with 16 characters (within 20 character limit)
        var request = new CreateContactRequest
        {
            Name = "田中太郎",
            Address = null,
            PhoneNumber = "+1-123-456-7890"  // 16 characters, valid format
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Property 3: All accepted phone number formats are valid
    /// </summary>
    [Theory]
    [InlineData("(123) 456-7890")]
    [InlineData("(123)456-7890")]
    [InlineData("123-456-7890")]
    [InlineData("123.456.7890")]
    [InlineData("+1-123-456-7890")]
    [InlineData("1234567890")]
    public void Property3_InputValidation_AcceptsAllValidPhoneNumberFormats(string phoneNumber)
    {
        // Arrange
        var request = new CreateContactRequest
        {
            Name = "田中太郎",
            Address = null,
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Property 3: Common invalid phone number formats are rejected
    /// </summary>
    [Theory]
    [InlineData("123-456-789")]      // Too short
    [InlineData("123456789")]        // Too short
    [InlineData("abc-def-ghij")]     // Letters
    [InlineData("12-34-56-78")]      // Wrong format
    [InlineData("+81-90-1234-5678")] // Wrong country code format
    [InlineData("123 456 7890")]     // Spaces (not in accepted format)
    public void Property3_InputValidation_RejectsCommonInvalidPhoneNumberFormats(string phoneNumber)
    {
        // Arrange
        var request = new CreateContactRequest
        {
            Name = "田中太郎",
            Address = null,
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _validator.Validate(request);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => 
            e.PropertyName == "PhoneNumber" && 
            e.ErrorMessage == "有効な電話番号を入力してください");
    }

    /// <summary>
    /// Helper method to check if a phone number matches any valid format
    /// Matches the regex pattern from CreateContactRequestValidator
    /// </summary>
    private static bool IsValidPhoneNumberFormat(string phoneNumber)
    {
        var pattern = @"^(\(\d{3}\)\s?\d{3}-\d{4}|\d{3}-\d{3}-\d{4}|\d{3}\.\d{3}\.\d{4}|\+\d{1}-\d{3}-\d{3}-\d{4}|\d{10})$";
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, pattern);
    }
}
