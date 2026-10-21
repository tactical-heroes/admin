using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Api;
using TacticalHeroes.Admin.Shared.Authorization;

namespace TacticalHeroes.Admin.Modules.Compendium;

public static class CompendiumAdminModule
{
    public static Assembly Assembly { get; } = typeof(CompendiumAdminModule).Assembly;

    public static IReadOnlyList<AdminNavigationGroup> NavigationGroups { get; } =
    [
        new(
            "Справочники",
            [
                new(
                    "Фракции",
                    CompendiumRoutes.Factions,
                    Icons.Material.Filled.Flag,
                    Permissions.Factions.Read),
                new(
                    "Герои",
                    CompendiumRoutes.Heroes,
                    Icons.Material.Filled.Person,
                    Permissions.Heroes.Read),
                new(
                    "Юниты",
                    CompendiumRoutes.Units,
                    Icons.Material.Filled.Groups,
                    Permissions.Units.Read),
            ]),
    ];

    public static IServiceCollection AddCompendiumAdminModule(
        this IServiceCollection services)
    {
        services.AddPermissionPolicies(Permissions.All);
        services.AddScoped<FactionListApi>();
        services.AddScoped<CreateFactionApi>();
        services.AddScoped<UpdateFactionApi>();
        services.AddScoped<FactionOptionsApi>();
        services.AddScoped<HeroListApi>();
        services.AddScoped<CreateHeroApi>();
        services.AddScoped<UpdateHeroApi>();
        services.AddScoped<UnitListApi>();
        services.AddScoped<CreateUnitApi>();
        services.AddScoped<UpdateUnitApi>();

        return services;
    }
}
