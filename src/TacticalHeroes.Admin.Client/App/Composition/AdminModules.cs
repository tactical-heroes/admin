using System.Reflection;
using TacticalHeroes.Admin.Modules.Compendium;
using TacticalHeroes.Admin.Modules.Identity;
using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Client.App.Composition;

public static class AdminModules
{
    public static IReadOnlyList<Assembly> Assemblies { get; } =
    [
        CompendiumAdminModule.Assembly,
        IdentityAdminModule.Assembly,
    ];

    public static IReadOnlyList<AdminNavigationGroup> NavigationGroups { get; } =
    [
        .. CompendiumAdminModule.NavigationGroups,
        .. IdentityAdminModule.NavigationGroups,
    ];

    internal static IServiceCollection AddAdminModules(
        this IServiceCollection services)
    {
        services.AddCompendiumAdminModule();
        services.AddIdentityAdminModule();

        return services;
    }
}
