using FluentValidation;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

public sealed class ClaimValueValidator : MudFormValidator<ClaimValue>
{
    public ClaimValueValidator()
    {
        RuleFor(claim => claim.Type)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Укажите тип атрибута")
            .Must(type => string.Equals(type?.Trim(), "permission", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Можно назначать только разрешения");

        RuleFor(claim => claim.Value)
            .NotEmpty()
            .WithMessage("Укажите значение атрибута");
    }
}
