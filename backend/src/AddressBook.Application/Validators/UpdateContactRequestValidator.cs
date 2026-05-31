using AddressBook.Application.DTOs;
using FluentValidation;

namespace AddressBook.Application.Validators;

public class UpdateContactRequestValidator : AbstractValidator<UpdateContactRequest>
{
    public UpdateContactRequestValidator()
    {
        // Name validation - Requirements 4.3, 8.3
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("名前は必須です")
            .Length(1, 100)
            .WithMessage("名前は1文字以上100文字以下で入力してください");

        // Address validation - Requirements 4.3, 8.3
        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("住所は500文字以下で入力してください")
            .When(x => !string.IsNullOrEmpty(x.Address));

        // Phone number validation - Requirements 4.3, 8.1, 8.3
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithMessage("電話番号は20文字以下で入力してください")
            .Matches(@"^(\(\d{2,4}\)\s?\d{1,4}-\d{4}|\d{2,4}-\d{1,4}-\d{4}|\d{2,4}\.\d{1,4}\.\d{4}|\+1-\d{3}-\d{3}-\d{4}|\+81-\d{1}-\d{4}-\d{4}|\d{10,11})$")
            .WithMessage("有効な電話番号を入力してください")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
