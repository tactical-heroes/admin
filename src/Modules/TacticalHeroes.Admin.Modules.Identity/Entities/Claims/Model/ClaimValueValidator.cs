using FluentValidation;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

public sealed class ClaimValueValidator : MudFormValidator<ClaimValue>
{
    public ClaimValueValidator()
    {
        RuleFor(claim => claim.Type)
            .NotEmpty()
            .WithMessage("Укажите тип атрибута")
            .Must(type => !string.Equals(type?.Trim(), "section", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Клеймы разделов больше не поддерживаются");

        RuleFor(claim => claim.Value)
            .NotEmpty()
            .WithMessage("Укажите значение атрибута");
    }
}
