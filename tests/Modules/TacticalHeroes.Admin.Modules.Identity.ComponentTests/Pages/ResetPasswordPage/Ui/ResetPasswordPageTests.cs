using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using ResetPasswordPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Ui.ResetPasswordPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.ResetPasswordPage.Ui;

public sealed class ResetPasswordPageTests : AuthenticationComponentTestContext
{
    [Fact(DisplayName = "Render should read email link parameters when query contains an encoded token")]
    public void Render_Should_ReadEmailLinkParameters_When_QueryContainsAnEncodedToken()
    {
        var userId = Guid.NewGuid();
        const string token = "token/+==&?% value";
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            IdentityRoutes.ResetPasswordPage(userId, token));

        var component = Render<ResetPasswordPageComponent>();

        component.Instance.UserId.ShouldBe(userId);
        component.Instance.PasswordResetToken.ShouldBe(token);
        component.Find("#reset-password").ShouldNotBeNull();
        _handler.PostCount.ShouldBe(0);
    }

    [Theory(DisplayName = "Render should reject email link when query parameters are missing")]
    [InlineData("")]
    [InlineData("?userId=19641d4e-0c67-4892-a952-7eb71725a064")]
    [InlineData("?passwordResetToken=token")]
    [InlineData("?userId=19641d4e-0c67-4892-a952-7eb71725a064&passwordResetToken=%20")]
    public void Render_Should_RejectEmailLink_When_QueryParametersAreMissing(string query)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(IdentityRoutes.ResetPassword + query);

        var component = Render<ResetPasswordPageComponent>();

        component.Find("h1").TextContent.ShouldBe("Ссылка недействительна");
        _handler.PostCount.ShouldBe(0);
    }

    [Fact(DisplayName = "TogglePasswordVisibility should toggle input type when button is clicked")]
    public void TogglePasswordVisibility_Should_ToggleInputType_When_ButtonIsClicked()
    {
        var component = RenderResetPasswordPage();

        component.FindAll("button[aria-label='Показать пароль']")[0].Click();

        component.Find("#reset-password").GetAttribute("type").ShouldBe("text");
        component.Find("button[aria-label='Скрыть пароль']").Click();

        component.Find("#reset-password").GetAttribute("type").ShouldBe("password");
    }

    [Fact(DisplayName = "TogglePasswordConfirmationVisibility should toggle input type when button is clicked")]
    public void TogglePasswordConfirmationVisibility_Should_ToggleInputType_When_ButtonIsClicked()
    {
        var component = RenderResetPasswordPage();

        component.FindAll("button[aria-label='Показать пароль']")[1].Click();

        component.Find("#reset-password-confirmation").GetAttribute("type").ShouldBe("text");
        component.Find("button[aria-label='Скрыть пароль']").Click();

        component.Find("#reset-password-confirmation").GetAttribute("type").ShouldBe("password");
    }

    [Fact(DisplayName = "SubmitAsync should display validation errors when model is empty")]
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

    [Fact(DisplayName = "SubmitAsync should submit when model is valid")]
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
