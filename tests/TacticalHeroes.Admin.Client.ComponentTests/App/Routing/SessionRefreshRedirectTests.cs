using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Api.Authentication;
using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Routing;

public sealed class SessionRefreshRedirectTests : ClientComponentTestContext
{
    [Theory(DisplayName = "Render should refresh session when signal arrives")]
    [InlineData("/")]
    [InlineData("/users/2f56eb88-8f57-47ef-884f-eae99d5f4ab8?tab=roles#permissions")]
    [InlineData("/users?page=2&email=admin%40example.test#list")]
    public async Task HandleRefreshRequested_Should_RefreshSession_When_SignalArrives(string currentPath)
    {
        var navigation = (BunitNavigationManager)Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(currentPath);
        var component = Render<SessionRefreshRedirect>();
        var state = Services.GetRequiredService<SessionRefreshState>();

        await component.InvokeAsync(state.RequireRefresh);

        component.WaitForAssertion(() =>
        {
            var uri = new Uri(navigation.Uri);
            uri.AbsolutePath.ShouldBe(IdentityRoutes.AuthenticationRefresh);
            Uri.UnescapeDataString(uri.Query["?returnUrl=".Length..]).ShouldBe(currentPath);
            navigation.History.First().Options.ForceLoad.ShouldBeTrue();
        });
    }

    [Fact(DisplayName = "Render should refresh session when signal arrived before rendering")]
    public void OnAfterRender_Should_RefreshSession_When_SignalArrivedBeforeRendering()
    {
        Services.GetRequiredService<SessionRefreshState>().RequireRefresh();

        Render<SessionRefreshRedirect>();

        Services.GetRequiredService<NavigationManager>().Uri
            .ShouldEndWith($"{IdentityRoutes.AuthenticationRefresh}?returnUrl=%2F");
    }

    [Fact(DisplayName = "NavigateTo should preserve refresh when save attempts normal navigation")]
    public async Task HandleLocationChanging_Should_PreserveRefresh_When_SaveAttemptsNormalNavigation()
    {
        const string currentPath = "/users/2f56eb88-8f57-47ef-884f-eae99d5f4ab8?tab=roles#permissions";
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(currentPath);
        var component = Render<SessionRefreshRedirect>();
        var state = Services.GetRequiredService<SessionRefreshState>();

        await component.InvokeAsync(() =>
        {
            state.RequireRefresh();
            navigation.NavigateTo(IdentityRoutes.Users);
        });

        component.WaitForAssertion(() =>
        {
            var uri = new Uri(navigation.Uri);
            uri.AbsolutePath.ShouldBe(IdentityRoutes.AuthenticationRefresh);
            Uri.UnescapeDataString(uri.Query["?returnUrl=".Length..]).ShouldBe(currentPath);
        });
    }

    [Fact(DisplayName = "NavigateTo should keep normal navigation when refresh is not required")]
    public void OnInitialized_Should_KeepNormalNavigation_When_RefreshIsNotRequired()
    {
        Render<SessionRefreshRedirect>();
        var navigation = Services.GetRequiredService<NavigationManager>();

        navigation.NavigateTo(IdentityRoutes.Users);

        navigation.Uri.ShouldEndWith(IdentityRoutes.Users);
    }

    [Fact(DisplayName = "RefreshSession should navigate once when multiple signals arrive")]
    public async Task RefreshSession_Should_NavigateOnce_When_MultipleSignalsArrive()
    {
        var component = Render<SessionRefreshRedirect>();
        var state = Services.GetRequiredService<SessionRefreshState>();
        var navigation = (BunitNavigationManager)Services.GetRequiredService<NavigationManager>();

        await component.InvokeAsync(() =>
        {
            state.RequireRefresh();
            state.RequireRefresh();
        });

        component.WaitForAssertion(() => navigation.History.ShouldHaveSingleItem()
            .Uri.ShouldBe($"{IdentityRoutes.AuthenticationRefresh}?returnUrl=%2F"));
    }

    [Fact(DisplayName = "Dispose should stop observing when component is disposed")]
    public void Dispose_Should_StopObserving_When_ComponentIsDisposed()
    {
        var component = Render<SessionRefreshRedirect>();
        var state = Services.GetRequiredService<SessionRefreshState>();
        var navigation = Services.GetRequiredService<NavigationManager>();
        string originalUri = navigation.Uri;

        component.Instance.Dispose();
        state.RequireRefresh();

        navigation.Uri.ShouldBe(originalUri);
    }
}
