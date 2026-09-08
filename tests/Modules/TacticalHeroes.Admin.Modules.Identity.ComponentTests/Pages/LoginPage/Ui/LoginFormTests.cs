using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.LoginPage.Ui;

public sealed class LoginFormTests : AuthenticationComponentTestContext
{
    [Theory(DisplayName = "Login mode links preserve the authorization return URL")]
    [InlineData(LoginMode.Register)]
    [InlineData(LoginMode.Recover)]
    [InlineData(LoginMode.Confirmation)]
    public void BuildModeHref_Should_PreserveReturnUrl_When_ModeLinkIsRendered(LoginMode mode)
    {
        const string returnUrl = "/connect/authorize?request_uri=urn:test&client_id=admin";

        var component = Render<LoginForm>(parameters => parameters
            .Add(form => form.ReturnUrl, returnUrl));

        component.FindAll("a").Select(link => link.GetAttribute("href"))
            .ShouldContain(IdentityRoutes.LoginPage(returnUrl, mode));
        component.Find("form").GetAttribute("action").ShouldBe(IdentityRoutes.AuthenticationSignIn);
        component.Find("input[name='ReturnUrl']").GetAttribute("value").ShouldBe(returnUrl);
    }


    [Fact(DisplayName = "TogglePasswordVisibility reveals and hides only its password field")]
    public void TogglePasswordVisibility_Should_ToggleInputType_When_ButtonIsClicked()
    {
        var component = Render<LoginForm>(parameters => parameters.Add(form => form.ReturnUrl, "/connect/authorize?client_id=admin"));

        component.FindAll("button[aria-label='Показать пароль']")[0].Click();

        component.Find("#login-password").GetAttribute("type").ShouldBe("text");
        component.Find("button[aria-label='Скрыть пароль']").Click();

        component.Find("#login-password").GetAttribute("type").ShouldBe("password");
    }

    [Fact(DisplayName = "Login form displays the authentication error display name")]
    public void Render_Should_DisplayErrorMessage_When_AuthenticationErrorIsProvided()
    {
        var component = Render<LoginForm>(parameters => parameters
            .Add(form => form.Error, AuthenticationError.Unavailable));

        component.Markup.ShouldContain(
            AuthenticationError.Unavailable.GetDisplayName());
    }
}
