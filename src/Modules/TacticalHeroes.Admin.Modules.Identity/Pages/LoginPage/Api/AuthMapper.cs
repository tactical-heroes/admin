using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;

internal static class AuthMapper
{
    public static Guid ToId(RegisterUserResponse response)
    {
        return response.Id!.Value;
    }
}
