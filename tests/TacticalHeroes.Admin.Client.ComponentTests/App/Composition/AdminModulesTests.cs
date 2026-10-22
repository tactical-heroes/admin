using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Client.App.Composition;
using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Composition;

public sealed class AdminModulesTests
{
    [Fact(DisplayName = "AddTacticalHeroesAdminClient should expose menu order when navigation groups are resolved")]
    public void AddTacticalHeroesAdminClient_Should_ExposeMenuOrder_When_NavigationGroupsAreResolved()
    {
        var services = new ServiceCollection();
        services.AddTacticalHeroesAdminClient(new ConfigurationBuilder().Build(), new Uri("https://localhost"));
        using var provider = services.BuildServiceProvider();

        var groups = provider.GetRequiredService<IEnumerable<AdminNavigationGroup>>();

        groups.ShouldBe(AdminModules.NavigationGroups);
    }
}
