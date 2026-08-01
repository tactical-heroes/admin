using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.Users;

public partial class UsersPage
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    [SupplyParameterFromQuery(Name = "email")]
    public string? Email { get; set; }

    private int CurrentPageNumber => PageNumber is > 0
        ? PageNumber.Value
        : 1;

    private void ApplyEmailFilter(string? email)
    {
        Navigation.NavigateTo(IdentityRoutes.UsersPage(email));
    }

    private void ChangePage(int pageNumber)
    {
        Navigation.NavigateTo(IdentityRoutes.UsersPage(Email, pageNumber));
    }
}
