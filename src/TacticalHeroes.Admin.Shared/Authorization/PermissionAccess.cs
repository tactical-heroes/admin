using System.Security.Claims;

using Microsoft.Extensions.DependencyInjection;

namespace TacticalHeroes.Admin.Shared.Authorization;

public static class PermissionAccess
{
    public const string ClaimType = "permission";

    public static bool HasPermission(ClaimsPrincipal user, string permission) =>
        user.Identity?.IsAuthenticated == true && user.HasClaim(ClaimType, permission);

    public static IServiceCollection AddPermissionPolicies(
        this IServiceCollection services,
        IEnumerable<string> permissions)
    {
        services.AddAuthorizationCore(options =>
        {
            foreach (string permission in permissions)
            {
                options.AddPolicy(permission, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimType, permission));
            }
        });

        return services;
    }
}
