using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using LoginPageComponent =
    TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui.LoginPage;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Pages.LoginPage.Ui;

public sealed class LoginPageTests : AuthenticationComponentTestContext
{
    [Theory(DisplayName = "Render should bind query and display title when mode is provided")]
    [InlineData("register", "Register - " + Branding.GameName)]
    [InlineData("confirmation", "Confirm email - " + Branding.GameName)]
    [InlineData("recover", "Recover access - " + Branding.GameName)]
    [InlineData("", "Sign in - " + Branding.GameName)]
    [InlineData("unknown", "Sign in - " + Branding.GameName)]
    public void Render_Should_BindQueryAndDisplayTitle_When_ModeIsProvided(
        string mode,
        string expectedTitle)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            $"/login?returnUrl=%2Fusers&error=unavailable&mode={mode}");
        var head = Render<HeadOutlet>();

        var component = Render<LoginPageComponent>();

        component.Instance.ReturnUrl.ShouldBe("/users");
        component.Instance.Error.ShouldBe("unavailable");
        component.Instance.Mode.ShouldBe(mode);
        head.Find("title").TextContent.ShouldBe(expectedTitle);
        component.Find(".auth-brand-title").GetAttribute("aria-label")
            .ShouldBe(Branding.GameName);
        string.Join(" ", component.FindAll(".auth-brand-title span")
            .Select(span => span.TextContent)).ShouldBe(Branding.GameName);
    }
}
