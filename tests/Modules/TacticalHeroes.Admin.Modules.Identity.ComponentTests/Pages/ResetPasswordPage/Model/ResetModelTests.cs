using FluentValidation.Results;

using TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.ResetPasswordPage.Model;

public sealed class ResetModelTests
{
    [Fact(DisplayName = "Stores password values")]
    public void ResetModel_Should_StoreValues_When_ValuesAreAssigned()
    {
        var model = new ResetModel
        {
            Password = "secret-password",
            PasswordConfirmation = "secret-password",
        };

        model.Password.ShouldBe("secret-password");
        model.PasswordConfirmation.ShouldBe("secret-password");
    }

    [Fact(DisplayName = "Validates required password values")]
    public void ResetModelValidator_Should_ReturnRequiredErrors_When_ModelIsEmpty()
    {
        var model = new ResetModel();

        ValidationResult result = new ResetModelValidator().Validate(model);

        result.Errors.ShouldContain(error =>
            error.ErrorMessage == "Укажите новый пароль.");
        result.Errors.ShouldContain(error => error.ErrorMessage == "Повторите новый пароль.");
    }

    [Fact(DisplayName = "Validates password value formats")]
    public void ResetModelValidator_Should_ReturnFormatErrors_When_ValuesAreInvalid()
    {
        var model = new ResetModel
        {
            Password = "short",
            PasswordConfirmation = "different",
        };

        ValidationResult result = new ResetModelValidator().Validate(model);

        result.Errors.ShouldContain(error =>
            error.ErrorMessage == "Пароль должен содержать минимум 8 символов.");
        result.Errors.ShouldContain(error => error.ErrorMessage == "Пароли не совпадают.");
    }

    [Fact(DisplayName = "Accepts valid password values")]
    public void ResetModelValidator_Should_ReturnNoErrors_When_ModelIsValid()
    {
        var model = new ResetModel
        {
            Password = "secret-password",
            PasswordConfirmation = "secret-password",
        };

        ValidationResult result = new ResetModelValidator().Validate(model);

        result.IsValid.ShouldBeTrue();
    }
}
