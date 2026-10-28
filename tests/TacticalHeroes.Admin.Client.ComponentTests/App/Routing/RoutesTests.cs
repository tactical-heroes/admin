using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Api.Authentication;
using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Client.Pages.Home;
using TacticalHeroes.Admin.Modules.Identity;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Ui;
using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Routing;

public sealed class RoutesTests : ClientComponentTestContext
{
    [Fact(DisplayName = "Render should isolate telemetry from unfinished loaders when route changes")]
    public async Task Render_Should_IsolateTelemetry_When_RouteChanges()
    {
        AddAuthorization().SetAuthorized("Administrator");
        var component = Render<Routes>();
        var previous = component.FindComponent<PageLoadTelemetry>().Instance;
        var previousState = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();
        var lateLoader = Render<PageLoadReporter>(parameters => parameters
            .AddCascadingValue(previousState)
            .Add(reporter => reporter.Loading, true));
        previousState.IsLoading.ShouldBeTrue();

        await component.InvokeAsync(() => Services.GetRequiredService<NavigationManager>().NavigateTo(IdentityRoutes.Login));

        component.WaitForAssertion(() => component.FindComponent<PageLoadTelemetry>().Instance.ShouldNotBeSameAs(previous));
        var current = component.FindComponent<CascadingValue<PageLoadState>>().Instance.Value.ShouldNotBeNull();
        lateLoader.Render(parameters => parameters
            .Add(reporter => reporter.Loading, false)
            .Add(reporter => reporter.HasError, true));
        previousState.HasError.ShouldBeTrue();
        current.IsLoading.ShouldBeFalse();
        current.HasError.ShouldBeFalse();
    }

    [Fact(DisplayName = "Render should keep login page when anonymous request rejects credentials")]
    public async Task Render_Should_KeepLoginPage_When_AnonymousRequestRejectsCredentials()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(IdentityRoutes.Login);
        var component = Render<Routes>();

        await component.InvokeAsync(Services.GetRequiredService<SessionRefreshState>().RequireRefresh);

        component.FindComponent<LoginPage>().ShouldNotBeNull();
        navigation.Uri.ShouldEndWith(IdentityRoutes.Login);
        component.FindComponents<SessionRefreshRedirect>().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Render should refresh session when authenticated request rejects session")]
    public async Task Render_Should_RefreshSession_When_AuthenticatedRequestRejectsSession()
    {
        AddAuthorization().SetAuthorized("Reader");
        var navigation = Services.GetRequiredService<NavigationManager>();
        var component = Render<Routes>();

        await component.InvokeAsync(Services.GetRequiredService<SessionRefreshState>().RequireRefresh);

        component.WaitForAssertion(() => navigation.Uri.ShouldContain(IdentityRoutes.AuthenticationRefresh));
    }

    [Fact(DisplayName = "Render should deny access without redirecting when authenticated user lacks permission")]
    public void Render_Should_DenyAccess_When_AuthenticatedUserLacksPermission()
    {
        var authorization = AddAuthorization();
        authorization.SetAuthorized("Reader");
        authorization.SetPolicies();
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(IdentityRoutes.Roles);

        var component = Render<Routes>();

        component.Markup.ShouldContain("Недостаточно прав");
        navigation.Uri.ShouldEndWith(IdentityRoutes.Roles);
    }

    [Fact(DisplayName = "Render should show home when user is authenticated")]
    public void Render_Should_ShowHome_When_UserIsAuthenticated()
    {
        AddAuthorization().SetAuthorized("Administrator");

        var component = Render<Routes>();

        component.FindComponent<HomePage>().ShouldNotBeNull();
        component.Markup.ShouldContain("Добро пожаловать");
    }

    [Fact(DisplayName = "Render should show module page when module route is requested")]
    public void Render_Should_ShowModulePage_When_ModuleRouteIsRequested()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(IdentityRoutes.Login);

        var component = Render<Routes>();

        component.FindComponent<LoginPage>().ShouldNotBeNull();
        component.Markup.ShouldContain("Вход в аккаунт");
    }

    [Fact(DisplayName = "Render should challenge when user is anonymous")]
    public void Render_Should_Challenge_When_UserIsAnonymous()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        string returnUrl = navigation.Uri;

        Render<Routes>();

        navigation.Uri.ShouldEndWith(IdentityRoutes.Challenge(returnUrl));
    }
}
