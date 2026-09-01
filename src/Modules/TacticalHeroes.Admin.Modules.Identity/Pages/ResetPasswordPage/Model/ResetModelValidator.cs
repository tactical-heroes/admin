using FluentValidation;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Model;

public sealed class ResetModelValidator : MudFormValidator<ResetModel>
{
    public ResetModelValidator()
    {
        RuleFor(model => model.Password)
            .NotEmpty()
            .WithMessage("Укажите новый пароль.")
            .MinimumLength(8)
            .WithMessage("Пароль должен содержать минимум 8 символов.");

        RuleFor(model => model.PasswordConfirmation)
            .NotEmpty()
            .WithMessage("Повторите новый пароль.")
            .Equal(model => model.Password)
            .WithMessage("Пароли не совпадают.");
    }
}
