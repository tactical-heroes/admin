using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Modules.Identity.Entities.Auth.Api;

internal static class AuthMapper
{
    public static Guid ToId(RegisterUserResponse response)
    {
        return response.Id
            ?? throw new ArgumentNullException(nameof(response.Id));
    }
}
