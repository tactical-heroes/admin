using FluentValidation;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Model;

public sealed class UpdateUserFormModelValidator
    : MudFormValidator<UpdateUserFormModel>
{
    public UpdateUserFormModelValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty()
            .WithMessage("Укажите email")
            .MaximumLength(320)
            .WithMessage("Email не должен превышать 320 символов")
            .EmailAddress()
            .WithMessage("Укажите корректный email");

        RuleFor(user => user.UserName)
            .NotEmpty()
            .WithMessage("Укажите имя пользователя")
            .MaximumLength(256)
            .WithMessage("Имя пользователя не должно превышать 256 символов");

        RuleFor(user => user.Status)
            .NotEmpty()
            .WithMessage("Выберите статус");
    }
}
