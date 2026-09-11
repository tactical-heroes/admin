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

namespace TacticalHeroes.Admin.Modules.Compendium;

public static class CompendiumAdminModule
{
    public static Assembly Assembly { get; } = typeof(CompendiumAdminModule).Assembly;

    public static IReadOnlyList<AdminNavigationGroup> NavigationGroups { get; } =
    [
        new(
            "Справочник",
            [
                new(
                    "Фракции",
                    CompendiumRoutes.Factions,
                    Icons.Material.Filled.Flag),
                new(
                    "Герои",
                    CompendiumRoutes.Heroes,
                    Icons.Material.Filled.Person),
                new(
                    "Юниты",
                    CompendiumRoutes.Units,
                    Icons.Material.Filled.Groups),
            ]),
    ];

    public static IServiceCollection AddCompendiumAdminModule(
        this IServiceCollection services)
    {
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
