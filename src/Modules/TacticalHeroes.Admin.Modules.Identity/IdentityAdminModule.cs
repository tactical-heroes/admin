using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using MudBlazor;

using TacticalHeroes.Admin.Shared.Navigation;

using ConfirmEmailApi = TacticalHeroes.Admin.Modules.Identity.Pages.ConfirmEmailPage.Api.AuthApi;
using CreateRoleApi = TacticalHeroes.Admin.Modules.Identity.Pages.CreateRolePage.Api.RolesApi;
using CreateUserApi = TacticalHeroes.Admin.Modules.Identity.Pages.CreateUserPage.Api.UsersApi;
using LoginApi = TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api.AuthApi;
using ResetPasswordApi = TacticalHeroes.Admin.Modules.Identity.Pages.ResetPasswordPage.Api.AuthApi;
using RoleListApi = TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Api.RolesApi;
using UpdateRoleApi = TacticalHeroes.Admin.Modules.Identity.Pages.UpdateRolePage.Api.RolesApi;
using UpdateUserApi = TacticalHeroes.Admin.Modules.Identity.Pages.UpdateUserPage.Api.UsersApi;
using UserListApi = TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Api.UsersApi;

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
                    Icons.Material.Filled.Security),
                new(
                    "Пользователи",
                    IdentityRoutes.Users,
                    Icons.Material.Filled.Group),
            ]),
    ];

    public static IServiceCollection AddIdentityAdminModule(
        this IServiceCollection services)
    {
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
