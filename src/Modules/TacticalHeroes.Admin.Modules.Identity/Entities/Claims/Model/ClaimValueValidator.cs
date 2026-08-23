using FluentValidation;

using TacticalHeroes.Admin.Shared.Validation;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

public sealed class ClaimValueValidator : MudFormValidator<ClaimValue>
{
    public ClaimValueValidator()
    {
        RuleFor(claim => claim.Type)
            .NotEmpty()
            .WithMessage("Укажите тип атрибута");

        RuleFor(claim => claim.Value)
            .NotEmpty()
            .WithMessage("Укажите значение атрибута");
    }
}
