using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Client.App.Composition;
using TacticalHeroes.Admin.Client.App.Layout;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Layout;

public sealed class NavMenuTests : ClientComponentTestContext
{
    [Fact(DisplayName = "VisibleGroups should filter resources and modules when permissions are limited")]
    public void VisibleGroups_Should_FilterResourcesAndModules_When_PermissionsAreLimited()
    {
        var authorization = AddAuthorization();
        authorization.SetAuthorized("Reader");
        authorization.SetClaims(new System.Security.Claims.Claim("permission", "compendium.factions.read"));

        var component = Render<NavMenu>();

        component.FindAll("details").Count.ShouldBe(1);
        component.FindAll("a").Select(link => link.GetAttribute("href")).ShouldBe(["/", "/factions"]);
        authorization.SetClaims(new System.Security.Claims.Claim("section", "identity"));
        component.WaitForAssertion(() => component.FindAll("details").ShouldBeEmpty());
    }

    [Theory(DisplayName = "IsCurrentGroup should expand matching module when route is rendered")]
    [InlineData("/factions", true, false)]
    [InlineData("/heroes/new", true, false)]
    [InlineData("/USERS/123?email=test#details", false, true)]
    [InlineData("/users-extra", false, false)]
    [InlineData("/", false, false)]
    public void IsCurrentGroup_Should_ExpandMatchingModule_When_RouteIsRendered(string path, bool compendium, bool identity)
    {
        var authorization = AddAuthorization();
        authorization.SetAuthorized("Administrator");
        authorization.SetClaims([.. AdminModules.NavigationGroups.SelectMany(group => group.Items)
            .Select(item => new System.Security.Claims.Claim("permission", item.Permission))]);
        Services.GetRequiredService<NavigationManager>().NavigateTo(path);

        var component = Render<NavMenu>();

        component.FindAll("details").Select(group => group.HasAttribute("open")).ShouldBe([compendium, identity]);
        component.Find("nav").GetAttribute("aria-label").ShouldBe("Основная навигация");
    }
}
