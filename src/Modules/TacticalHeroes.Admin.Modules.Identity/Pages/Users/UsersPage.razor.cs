using Microsoft.AspNetCore.Components;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.Users;

public partial class UsersPage
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    [SupplyParameterFromQuery(Name = "email")]
    public string? Email { get; set; }

    [SupplyParameterFromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    private int CurrentPageNumber => PageNumber is > 0
        ? PageNumber.Value
        : 1;

    private int CurrentPageSize => PaginationOptions.NormalizePageSize(PageSize);

    private void ApplyEmailFilter(string? email)
    {
        Navigation.NavigateTo(IdentityRoutes.UsersPage(email, pageSize: CurrentPageSize));
    }

    private void ChangePage(int pageNumber)
    {
        Navigation.NavigateTo(IdentityRoutes.UsersPage(Email, pageNumber, CurrentPageSize));
    }

    private void ChangePageSize(int pageSize)
    {
        Navigation.NavigateTo(IdentityRoutes.UsersPage(Email, pageSize: pageSize));
    }
}
