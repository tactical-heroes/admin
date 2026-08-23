using FluentValidation;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Model;

public sealed class CreateRoleFormModelValidator
    : MudFormValidator<CreateRoleFormModel>
{
    public CreateRoleFormModelValidator()
    {
        RuleFor(role => role.Name)
            .NotEmpty()
            .WithMessage("Укажите название роли")
            .MaximumLength(128)
            .WithMessage("Название роли не должно превышать 128 символов");

        RuleForEach(role => role.Claims)
            .SetValidator(new ClaimValueValidator());
    }
}
