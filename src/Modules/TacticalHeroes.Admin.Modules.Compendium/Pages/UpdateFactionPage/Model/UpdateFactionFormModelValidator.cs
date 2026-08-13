using FluentValidation;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Model;

public sealed class UpdateFactionFormModelValidator
    : MudFormValidator<UpdateFactionFormModel>
{
    public UpdateFactionFormModelValidator()
    {
        RuleFor(faction => faction.Name)
            .NotEmpty()
            .WithMessage("Укажите название фракции")
            .MaximumLength(128)
            .WithMessage("Название фракции не должно превышать 128 символов");

        RuleFor(faction => faction.Description)
            .NotEmpty()
            .WithMessage("Укажите описание фракции")
            .MaximumLength(2000)
            .WithMessage("Описание фракции не должно превышать 2000 символов");
    }
}
