using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;
using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Modules.Identity;

public static class IdentityAdminModule
{
    public static Assembly Assembly { get; } = typeof(IdentityAdminModule).Assembly;

    public static IReadOnlyList<AdminNavigationGroup> NavigationGroups { get; } =
    [
        new(
            "Identity",
            [
                new(
                    "Роли",
                    "/roles",
                    Icons.Material.Filled.Security),
                new(
                    "Пользователи",
                    "/users",
                    Icons.Material.Filled.Group),
            ]),
    ];

    public static IServiceCollection AddIdentityAdminModule(
        this IServiceCollection services)
    {
        services.AddScoped<RolesApi>();
        services.AddScoped<UsersApi>();
        services.AddScoped<AuthenticationApi>();

        return services;
    }
}
