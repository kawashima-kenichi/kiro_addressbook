using AddressBook.Application.DTOs;
using FluentValidation;

namespace AddressBook.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("メールアドレスは必須です")
            .EmailAddress().WithMessage("有効なメールアドレスを入力してください");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("パスワードは必須です");
    }
}
