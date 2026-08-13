using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.LoginPage.Api;

internal static class LoginMapper
{
    public static Guid ToId(RegisterUserResponse response)
    {
        return response.Id!.Value;
    }
}
