using FluentValidation;

using Pl.Ui.Blazor.ViewModels;

namespace Pl.Ui.Blazor.Validators;

public sealed class UserViewModelValidator : AbstractValidator<UserViewModel>
{
    public UserViewModelValidator()
    {
        RuleFor(item => item.Name)
            .NotEmpty()
            .WithMessage("Введите имя.");

        RuleFor(item => item.Role)
            .NotNull()
            .WithMessage("Выберите роль.")
            .IsInEnum()
            .WithMessage("Выберите роль.");

        RuleFor(item => item.Email)
            .NotEmpty()
            .WithMessage("Введите Email.")
            .EmailAddress()
            .WithMessage("Некорректный Email.");

        RuleFor(item => item.Phone)
            .NotEmpty()
            .WithMessage("Введите телефон.");

        RuleFor(item => item.Birthday)
            .NotNull()
            .WithMessage("Выберите дату рождения.")
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Дата рождения не может быть в будущем.");
    }
}
