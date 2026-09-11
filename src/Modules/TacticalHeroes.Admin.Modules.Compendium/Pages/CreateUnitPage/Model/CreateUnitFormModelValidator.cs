using FluentValidation;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Model;

public sealed class CreateUnitFormModelValidator : MudFormValidator<CreateUnitFormModel>
{
    public CreateUnitFormModelValidator()
    {
        RuleFor(unit => unit.Name)
            .NotEmpty().WithMessage("Укажите имя юнита")
            .MaximumLength(128).WithMessage("Имя юнита не должно превышать 128 символов");

        RuleFor(unit => unit.Description)
            .NotEmpty().WithMessage("Укажите описание юнита")
            .MaximumLength(2000).WithMessage("Описание юнита не должно превышать 2000 символов");

        RuleFor(unit => unit.FactionId)
            .NotEmpty().WithMessage("Выберите фракцию");

        RuleFor(unit => unit.Attack)
            .GreaterThanOrEqualTo(0).WithMessage("Атака не может быть отрицательной");

        RuleFor(unit => unit.Defense)
            .GreaterThanOrEqualTo(0).WithMessage("Защита не может быть отрицательной");

        RuleFor(unit => unit.MinimumDamage)
            .GreaterThanOrEqualTo(0).WithMessage("Минимальный урон не может быть отрицательным");

        RuleFor(unit => unit.Health)
            .GreaterThan(0).WithMessage("Здоровье должно быть больше нуля");

        RuleFor(unit => unit.Speed)
            .GreaterThanOrEqualTo(0).WithMessage("Скорость не может быть отрицательной");

        RuleFor(unit => unit.Shots)
            .GreaterThan(0).WithMessage("Количество выстрелов должно быть больше нуля");

        RuleFor(unit => unit.RangedAttackRange)
            .GreaterThan(0).WithMessage("Дальность стрельбы должна быть больше нуля")
            .Must((unit, range) => unit.Shots.HasValue == range.HasValue)
            .WithMessage("Укажите количество выстрелов и дальность стрельбы вместе или оставьте оба поля пустыми");

        RuleFor(unit => unit.MaximumDamage)
            .GreaterThanOrEqualTo(0).WithMessage("Максимальный урон не может быть отрицательным")
            .GreaterThanOrEqualTo(unit => unit.MinimumDamage)
            .WithMessage("Максимальный урон не может быть меньше минимального");

        RuleFor(unit => unit.Initiative)
            .Must(value => double.IsFinite(value) && value >= 0)
            .WithMessage("Инициатива должна быть конечным неотрицательным числом");

        RuleFor(unit => unit.Morale)
            .InclusiveBetween(0, 5).WithMessage("Мораль должна быть от 0 до 5");

        RuleFor(unit => unit.Luck)
            .InclusiveBetween(0, 5).WithMessage("Удача должна быть от 0 до 5");
    }
}
