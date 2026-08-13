using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Client.App.Routing;

public partial class RedirectToLogin(NavigationManager navigation)
{
    protected override void OnInitialized()
    {
        navigation.NavigateTo(
            IdentityRoutes.Challenge(navigation.Uri),
            forceLoad: true);
    }
}
