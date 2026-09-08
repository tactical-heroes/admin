using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Client.App.Layout;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Layout;

public sealed class NavMenuTests : ClientComponentTestContext
{
    [Theory(DisplayName = "Navigation expands only the matching module and ignores URL query and fragment")]
    [InlineData("/factions", true, false)]
    [InlineData("/USERS/123?email=test#details", false, true)]
    [InlineData("/users-extra", false, false)]
    [InlineData("/", false, false)]
    public void IsCurrentGroup_Should_ExpandMatchingModule_When_RouteIsRendered(string path, bool compendium, bool identity)
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(path);

        var component = Render<NavMenu>();

        component.FindAll("details").Select(group => group.HasAttribute("open")).ShouldBe([compendium, identity]);
        component.Find("nav").GetAttribute("aria-label").ShouldBe("Основная навигация");
    }
}

