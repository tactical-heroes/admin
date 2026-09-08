using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.LoginPage.Ui;

public sealed class ResendConfirmationFormTests : AuthenticationComponentTestContext
{
    [Fact(DisplayName = "Confirmation form validates an empty model through MudForm")]
    public void SubmitAsync_Should_DisplayValidationError_When_ModelIsEmpty()
    {
        var component = Render<ResendConfirmationForm>();

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(0);
            component.Markup.ShouldContain("Укажите email.");
        });
    }

    [Fact(DisplayName = "Confirmation form submits a valid MudForm model")]
    public void SubmitAsync_Should_Submit_When_ModelIsValid()
    {
        var component = Render<ResendConfirmationForm>();
        component.Find("#confirmation-email").Input("admin@example.com");

        component.Find("button.auth-command").Click();

        component.WaitForAssertion(() =>
        {
            _handler.PostCount.ShouldBe(1);
            component.Markup.ShouldContain("новое письмо уже отправлено");
        });
    }
}
