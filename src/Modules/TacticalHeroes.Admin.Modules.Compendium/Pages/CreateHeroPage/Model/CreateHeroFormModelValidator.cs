using FluentValidation;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Model;

public sealed class CreateHeroFormModelValidator : MudFormValidator<CreateHeroFormModel>
{
    public CreateHeroFormModelValidator()
    {
        RuleFor(hero => hero.Name)
            .NotEmpty().WithMessage("Укажите имя героя")
            .MaximumLength(128).WithMessage("Имя героя не должно превышать 128 символов");

        RuleFor(hero => hero.Description)
            .NotEmpty().WithMessage("Укажите описание героя")
            .MaximumLength(2000).WithMessage("Описание героя не должно превышать 2000 символов");

        RuleFor(hero => hero.FactionId)
            .NotEmpty().WithMessage("Выберите фракцию");

        RuleFor(hero => hero.Attack)
            .GreaterThanOrEqualTo(0).WithMessage("Атака не может быть отрицательной");

        RuleFor(hero => hero.Defense)
            .GreaterThanOrEqualTo(0).WithMessage("Защита не может быть отрицательной");

        RuleFor(hero => hero.MinimumDamage)
            .GreaterThanOrEqualTo(0).WithMessage("Минимальный урон не может быть отрицательным");

        RuleFor(hero => hero.MaximumDamage)
            .GreaterThanOrEqualTo(0).WithMessage("Максимальный урон не может быть отрицательным")
            .GreaterThanOrEqualTo(hero => hero.MinimumDamage)
            .WithMessage("Максимальный урон не может быть меньше минимального");

        RuleFor(hero => hero.Initiative)
            .Must(value => double.IsFinite(value) && value >= 0)
            .WithMessage("Инициатива должна быть конечным неотрицательным числом");

        RuleFor(hero => hero.Morale)
            .InclusiveBetween(0, 5).WithMessage("Мораль должна быть от 0 до 5");

        RuleFor(hero => hero.Luck)
            .InclusiveBetween(0, 5).WithMessage("Удача должна быть от 0 до 5");
    }
}
