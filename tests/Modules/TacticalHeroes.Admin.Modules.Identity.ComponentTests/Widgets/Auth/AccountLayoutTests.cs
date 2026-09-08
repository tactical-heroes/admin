using MudBlazor;
using MudBlazor.Services;

using TacticalHeroes.Admin.Modules.Identity.Widgets.Auth;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Widgets.Auth;

public sealed class AccountLayoutTests : BunitContext
{
    [Fact(DisplayName = "The account layout renders its body with dialog and snackbar providers")]
    public void Render_Should_ComposeBodyAndProviders_When_ContentIsProvided()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var component = Render<AccountLayout>(parameters => parameters
            .Add(layout => layout.Body, "<h1>Sign in</h1>"));

        component.Find("main h1").TextContent.ShouldBe("Sign in");
        component.FindComponent<MudDialogProvider>().ShouldNotBeNull();
        component.FindComponent<MudSnackbarProvider>().ShouldNotBeNull();
        component.Find("#blazor-error-ui .reload").GetAttribute("href").ShouldBe(".");
    }
}
