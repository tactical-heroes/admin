using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.LoginPage.Ui;

public sealed class ForgotPasswordFormTests : AuthenticationComponentTestContext
{
    [Fact(DisplayName = "Forgot password form validates an empty model through MudForm")]
    public void SubmitAsync_Should_DisplayValidationError_When_ModelIsEmpty()
    {
        var component = Render<ForgotPasswordForm>();

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите email.");
        });
    }

    [Fact(DisplayName = "Forgot password form submits a valid MudForm model")]
    public void SubmitAsync_Should_Submit_When_ModelIsValid()
    {
        var component = Render<ForgotPasswordForm>();
        component.Find("#recovery-email").Input("admin@example.com");

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            component.Markup.ShouldContain("Если подтверждённый аккаунт существует");
        });
    }
}
