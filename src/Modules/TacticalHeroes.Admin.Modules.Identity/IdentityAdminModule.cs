using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Api;
using TacticalHeroes.Admin.Shared.Authorization;

namespace TacticalHeroes.Admin.Modules.Identity;

public static class IdentityAdminModule
{
    public static Assembly Assembly { get; } = typeof(IdentityAdminModule).Assembly;

    public static IReadOnlyList<AdminNavigationGroup> NavigationGroups { get; } =
    [
        new(
            "Управление доступом",
            [
                new(
                    "Роли",
                    IdentityRoutes.Roles,
                    Icons.Material.Filled.Security,
                    Permissions.Roles.Read),
                new(
                    "Пользователи",
                    IdentityRoutes.Users,
                    Icons.Material.Filled.Group,
                    Permissions.Users.Read),
            ]),
    ];

    public static IServiceCollection AddIdentityAdminModule(
        this IServiceCollection services)
    {
        services.AddPermissionPolicies(Permissions.All);
        services.AddScoped<IEnumerationProvider<UserStatus>, UserStatusApi>();
        services.AddScoped<ClaimCatalogApi>();
        services.AddScoped<RoleOptionsApi>();
        services.AddScoped<RoleListApi>();
        services.AddScoped<CreateRoleApi>();
        services.AddScoped<UpdateRoleApi>();
        services.AddScoped<UserListApi>();
        services.AddScoped<CreateUserApi>();
        services.AddScoped<UpdateUserApi>();
        services.AddScoped<LoginApi>();
        services.AddScoped<ConfirmEmailApi>();
        services.AddScoped<ResetPasswordApi>();

        return services;
    }
}
