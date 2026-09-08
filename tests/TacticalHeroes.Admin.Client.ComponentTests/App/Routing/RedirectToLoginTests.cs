using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Routing;

public sealed class RedirectToLoginTests : BunitContext
{
    [Fact(DisplayName = "OnInitialized should challenge with return url when component is rendered")]
    public void OnInitialized_Should_ChallengeWithReturnUrl_When_ComponentIsRendered()
    {
        var navigation = (BunitNavigationManager)Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/users?page=2&email=admin%40example.test");
        string returnUrl = navigation.Uri;

        Render<RedirectToLogin>();

        navigation.History.First().Uri.ShouldBe(IdentityRoutes.Challenge(returnUrl));
        navigation.History.First().Options.ForceLoad.ShouldBeTrue();
    }
}
