using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Client.Pages.Home;
using TacticalHeroes.Admin.Modules.Identity;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Routing;

public sealed class RoutesTests : ClientComponentTestContext
{
    [Fact(DisplayName = "The router renders the home page for an authenticated user")]
    public void Render_Should_ShowHome_When_UserIsAuthenticated()
    {
        AddAuthorization().SetAuthorized("Administrator");

        var component = Render<Routes>();

        component.FindComponent<HomePage>().ShouldNotBeNull();
        component.Markup.ShouldContain("Добро пожаловать");
    }

    [Fact(DisplayName = "The router discovers anonymous pages from module assemblies")]
    public void Render_Should_ShowModulePage_When_ModuleRouteIsRequested()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(IdentityRoutes.Login);

        var component = Render<Routes>();

        component.FindComponent<LoginPage>().ShouldNotBeNull();
        component.Markup.ShouldContain("Вход в аккаунт");
    }

    [Fact(DisplayName = "The router challenges an anonymous user requesting an authorized page")]
    public void Render_Should_Challenge_When_UserIsAnonymous()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        string returnUrl = navigation.Uri;

        Render<Routes>();

        navigation.Uri.ShouldEndWith(IdentityRoutes.Challenge(returnUrl));
    }
}
