using AddressBook.Application.DTOs;
using FluentValidation;

namespace AddressBook.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("メールアドレスは必須です")
            .EmailAddress().WithMessage("有効なメールアドレスを入力してください")
            .MaximumLength(255).WithMessage("メールアドレスは255文字以下で入力してください");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("パスワードは必須です")
            .MinimumLength(8).WithMessage("パスワードは8文字以上で入力してください")
            .Matches(@"(?=.*[a-z])").WithMessage("パスワードは大文字、小文字、数字、特殊文字を含む必要があります")
            .Matches(@"(?=.*[A-Z])").WithMessage("パスワードは大文字、小文字、数字、特殊文字を含む必要があります")
            .Matches(@"(?=.*\d)").WithMessage("パスワードは大文字、小文字、数字、特殊文字を含む必要があります")
            .Matches(@"(?=.*[@$!%*?&])").WithMessage("パスワードは大文字、小文字、数字、特殊文字を含む必要があります");
    }
}
