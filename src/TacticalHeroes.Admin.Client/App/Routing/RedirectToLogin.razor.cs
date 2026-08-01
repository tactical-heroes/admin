using Microsoft.AspNetCore.Components;
using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Client.App.Routing;

public partial class RedirectToLogin
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    protected override void OnInitialized()
    {
        Navigation.NavigateTo(
            IdentityRoutes.Challenge(Navigation.Uri),
            forceLoad: true);
    }
}
