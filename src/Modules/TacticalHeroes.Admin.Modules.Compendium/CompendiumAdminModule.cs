using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using MudBlazor;

using TacticalHeroes.Admin.Shared.Navigation;

using CreateFactionApi = TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api.FactionsApi;
using FactionListApi = TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api.FactionsApi;
using UpdateFactionApi = TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api.FactionsApi;

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
            ]),
    ];

    public static IServiceCollection AddCompendiumAdminModule(
        this IServiceCollection services)
    {
        services.AddScoped<FactionListApi>();
        services.AddScoped<CreateFactionApi>();
        services.AddScoped<UpdateFactionApi>();

        return services;
    }
}
