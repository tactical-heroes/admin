namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.ResetPasswordPage.Ui;

public sealed class ResetPasswordPageTests : AuthenticationComponentTestContext
{

    [Fact(DisplayName = "TogglePasswordVisibility reveals and hides only its password field")]
    public void TogglePasswordVisibility_Should_ToggleInputType_When_ButtonIsClicked()
    {
        var component = RenderResetPasswordPage();

        component.FindAll("button[aria-label='Показать пароль']")[0].Click();

        component.Find("#reset-password").GetAttribute("type").ShouldBe("text");
        component.Find("button[aria-label='Скрыть пароль']").Click();

        component.Find("#reset-password").GetAttribute("type").ShouldBe("password");
    }

    [Fact(DisplayName = "TogglePasswordConfirmationVisibility reveals and hides only its password field")]
    public void TogglePasswordConfirmationVisibility_Should_ToggleInputType_When_ButtonIsClicked()
    {
        var component = RenderResetPasswordPage();

        component.FindAll("button[aria-label='Показать пароль']")[1].Click();

        component.Find("#reset-password-confirmation").GetAttribute("type").ShouldBe("text");
        component.Find("button[aria-label='Скрыть пароль']").Click();

        component.Find("#reset-password-confirmation").GetAttribute("type").ShouldBe("password");
    }

    [Fact(DisplayName = "Reset password form validates an empty model through MudForm")]
    public void SubmitAsync_Should_DisplayValidationErrors_When_ModelIsEmpty()
    {
        var component = RenderResetPasswordPage();

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите новый пароль.");
            component.Markup.ShouldContain("Повторите новый пароль.");
        });
    }

    [Fact(DisplayName = "Reset password form submits a valid MudForm model")]
    public void SubmitAsync_Should_Submit_When_ModelIsValid()
    {
        var component = RenderResetPasswordPage();
        component.Find("#reset-password").Input("Password1!");
        component.Find("#reset-password-confirmation").Input("Password1!");

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            component.Markup.ShouldContain("Пароль изменён");
        });
    }
}
