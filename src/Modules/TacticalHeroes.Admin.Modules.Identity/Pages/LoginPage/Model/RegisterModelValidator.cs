using FluentValidation;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;

public sealed class RegisterModelValidator : MudFormValidator<RegisterModel>
{
    public RegisterModelValidator()
    {
        RuleFor(model => model.Email)
            .NotEmpty()
            .WithMessage("Укажите email.")
            .EmailAddress()
            .WithMessage("Укажите корректный email.");

        RuleFor(model => model.UserName)
            .NotEmpty()
            .WithMessage("Укажите имя пользователя.")
            .MinimumLength(2)
            .WithMessage("Имя пользователя слишком короткое.");

        RuleFor(model => model.Password)
            .NotEmpty()
            .WithMessage("Укажите пароль.")
            .MinimumLength(8)
            .WithMessage("Пароль должен содержать минимум 8 символов.");

        RuleFor(model => model.PasswordConfirmation)
            .NotEmpty()
            .WithMessage("Повторите пароль.")
            .Equal(model => model.Password)
            .WithMessage("Пароли не совпадают.");
    }
}
