using TacticalHeroes.Admin.Modules.Identity.Widgets.Authentication;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Widgets.Authentication;

public sealed class AuthenticationShellTests : BunitContext
{
    [Fact(DisplayName = "Renders brand and child content")]
    public void Render_Should_ShowBrandAndChildContent_When_ContentIsProvided()
    {
        var component = Render<AuthenticationShell>(parameters => parameters
            .AddChildContent("<p class=\"test-content\">Login form</p>"));

        component.Find(".auth-brand-title").TextContent.ShouldContain("Tactical");
        component.Find(".auth-brand-title").TextContent.ShouldContain("Heroes");
        component.Find(".test-content").TextContent.ShouldBe("Login form");
    }

    [Fact(DisplayName = "Does not render global navigation")]
    public void Render_Should_NotShowGlobalNavigation_When_ContentIsProvided()
    {
        var component = Render<AuthenticationShell>(parameters => parameters
            .AddChildContent("<p>Login form</p>"));

        component.FindAll("nav").ShouldBeEmpty();
        component.FindAll(".auth-navigation-link").ShouldBeEmpty();
    }
}
