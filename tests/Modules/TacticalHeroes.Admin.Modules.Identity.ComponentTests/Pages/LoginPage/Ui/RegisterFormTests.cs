using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.LoginPage.Ui;

public sealed class RegisterFormTests : AuthenticationComponentTestContext
{
    [Fact(DisplayName = "TogglePasswordVisibility reveals and hides only its password field")]
    public void TogglePasswordVisibility_Should_ToggleInputType_When_ButtonIsClicked()
    {
        var component = Render<RegisterForm>();

        component.FindAll("button[aria-label='Показать пароль']")[0].Click();

        component.Find("#register-password").GetAttribute("type").ShouldBe("text");
        component.Find("button[aria-label='Скрыть пароль']").Click();

        component.Find("#register-password").GetAttribute("type").ShouldBe("password");
    }

    [Fact(DisplayName = "TogglePasswordConfirmationVisibility reveals and hides only its password field")]
    public void TogglePasswordConfirmationVisibility_Should_ToggleInputType_When_ButtonIsClicked()
    {
        var component = Render<RegisterForm>();

        component.FindAll("button[aria-label='Показать пароль']")[1].Click();

        component.Find("#register-password-confirmation").GetAttribute("type").ShouldBe("text");
        component.Find("button[aria-label='Скрыть пароль']").Click();

        component.Find("#register-password-confirmation").GetAttribute("type").ShouldBe("password");
    }

    [Fact(DisplayName = "Register form validates an empty model through MudForm")]
    public void SubmitAsync_Should_DisplayValidationErrors_When_ModelIsEmpty()
    {
        var component = Render<RegisterForm>();

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите email.");
            component.Markup.ShouldContain("Укажите имя пользователя.");
            component.Markup.ShouldContain("Укажите пароль.");
            component.Markup.ShouldContain("Повторите пароль.");
        });
    }

    [Fact(DisplayName = "Register form submits a valid MudForm model")]
    public void SubmitAsync_Should_Submit_When_ModelIsValid()
    {
        var component = Render<RegisterForm>();
        component.Find("#register-email").Input("admin@example.com");
        component.Find("#register-user-name").Input("Administrator");
        component.Find("#register-password").Input("Password1!");
        component.Find("#register-password-confirmation").Input("Password1!");

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            component.Markup.ShouldContain("Аккаунт создан.");
        });
    }
}
