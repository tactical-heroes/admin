using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

using TacticalHeroes.Admin.Shared.Authorization;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Session;

internal static class SessionPermissionsVersion
{
    internal const string HeaderName = "X-Session-Permissions-Version";

    internal static string Create(ClaimsPrincipal user)
    {
        string[] values = [.. user.FindAll(PermissionAccess.ClaimType)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Prepend(user.FindFirst("sub")?.Value ?? string.Empty)];
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(values)));
    }
}
