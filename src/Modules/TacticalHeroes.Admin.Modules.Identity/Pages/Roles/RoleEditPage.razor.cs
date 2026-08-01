using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.Roles;

public partial class RoleEditPage
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    private void NavigateToList()
    {
        Navigation.NavigateTo(IdentityRoutes.Roles);
    }
}
