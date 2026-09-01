using FluentValidation;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;

public sealed class EmailModelValidator : MudFormValidator<EmailModel>
{
    public EmailModelValidator()
    {
        RuleFor(model => model.Email)
            .NotEmpty()
            .WithMessage("Укажите email.")
            .EmailAddress()
            .WithMessage("Укажите корректный email.");
    }
}
