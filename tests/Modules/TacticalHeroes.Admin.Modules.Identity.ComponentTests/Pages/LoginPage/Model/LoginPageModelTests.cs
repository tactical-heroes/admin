using FluentValidation.Results;

using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.LoginPage.Model;

public sealed class LoginPageModelTests
{
    [Fact(DisplayName = "EmailModel should store email when value is assigned")]
    public void EmailModel_Should_StoreEmail_When_ValueIsAssigned()
    {
        var model = new EmailModel
        {
            Email = "hero@example.com",
        };

        model.Email.ShouldBe("hero@example.com");
    }

    [Fact(DisplayName = "RegisterModel should store values when values are assigned")]
    public void RegisterModel_Should_StoreValues_When_ValuesAreAssigned()
    {
        var model = new RegisterModel
        {
            Email = "hero@example.com",
            UserName = "hero",
            Password = "secret-password",
            PasswordConfirmation = "secret-password",
        };

        model.Email.ShouldBe("hero@example.com");
        model.UserName.ShouldBe("hero");
        model.Password.ShouldBe("secret-password");
        model.PasswordConfirmation.ShouldBe("secret-password");
    }

    [Theory(DisplayName = "EmailModelValidator should return expected error when email is invalid")]
    [InlineData("", "Укажите email.")]
    [InlineData("invalid-email", "Укажите корректный email.")]
    public void EmailModelValidator_Should_ReturnExpectedError_When_EmailIsInvalid(
        string email,
        string expectedError)
    {
        var model = new EmailModel { Email = email };

        ValidationResult result = new EmailModelValidator().Validate(model);

        result.Errors.ShouldContain(error =>
            error.PropertyName == nameof(EmailModel.Email) &&
            error.ErrorMessage == expectedError);
    }

    [Fact(DisplayName = "EmailModelValidator should return no errors when email is valid")]
    public void EmailModelValidator_Should_ReturnNoErrors_When_EmailIsValid()
    {
        var model = new EmailModel { Email = "hero@example.com" };

        ValidationResult result = new EmailModelValidator().Validate(model);

        result.IsValid.ShouldBeTrue();
    }

    [Fact(DisplayName = "RegisterModelValidator should return required errors when model is empty")]
    public void RegisterModelValidator_Should_ReturnRequiredErrors_When_ModelIsEmpty()
    {
        var model = new RegisterModel();

        ValidationResult result = new RegisterModelValidator().Validate(model);

        result.Errors.ShouldContain(error => error.ErrorMessage == "Укажите email.");
        result.Errors.ShouldContain(error =>
            error.ErrorMessage == "Укажите имя пользователя.");
        result.Errors.ShouldContain(error => error.ErrorMessage == "Укажите пароль.");
        result.Errors.ShouldContain(error => error.ErrorMessage == "Повторите пароль.");
    }

    [Fact(DisplayName = "RegisterModelValidator should return format errors when values are invalid")]
    public void RegisterModelValidator_Should_ReturnFormatErrors_When_ValuesAreInvalid()
    {
        var model = new RegisterModel
        {
            Email = "invalid-email",
            UserName = "h",
            Password = "short",
            PasswordConfirmation = "different",
        };

        ValidationResult result = new RegisterModelValidator().Validate(model);

        result.Errors.ShouldContain(error =>
            error.ErrorMessage == "Укажите корректный email.");
        result.Errors.ShouldContain(error =>
            error.ErrorMessage == "Имя пользователя слишком короткое.");
        result.Errors.ShouldContain(error =>
            error.ErrorMessage == "Пароль должен содержать минимум 8 символов.");
        result.Errors.ShouldContain(error => error.ErrorMessage == "Пароли не совпадают.");
    }

    [Fact(DisplayName = "RegisterModelValidator should return no errors when model is valid")]
    public void RegisterModelValidator_Should_ReturnNoErrors_When_ModelIsValid()
    {
        var model = new RegisterModel
        {
            Email = "hero@example.com",
            UserName = "hero",
            Password = "secret-password",
            PasswordConfirmation = "secret-password",
        };

        ValidationResult result = new RegisterModelValidator().Validate(model);

        result.IsValid.ShouldBeTrue();
    }
}
