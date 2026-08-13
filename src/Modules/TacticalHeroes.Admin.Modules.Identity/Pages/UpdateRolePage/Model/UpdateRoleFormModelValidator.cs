using FluentValidation;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Model;

public sealed class UpdateRoleFormModelValidator
    : MudFormValidator<UpdateRoleFormModel>
{
    public UpdateRoleFormModelValidator()
    {
        RuleFor(role => role.Name)
            .NotEmpty()
            .WithMessage("Укажите название роли")
            .MaximumLength(128)
            .WithMessage("Название роли не должно превышать 128 символов");
    }
}
