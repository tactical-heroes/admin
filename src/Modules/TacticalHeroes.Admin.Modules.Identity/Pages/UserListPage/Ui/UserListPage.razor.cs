using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Ui;

public partial class UserListPage(
    UserListApi userListApi,
    NavigationManager navigation)
    : MudPagedListComponentBase<UserListItem, UserListFilter>(
        userListApi.GetPageAsync,
        IdentityRoutes.Users,
        navigation)
{
    [SupplyParameterFromQuery(Name = "email")]
    public string? Email { get; set; }

    protected override UserListFilter AppliedFilter => new() { Email = Email };

    private Task<Result> DeleteUserAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return userListApi.DeleteAsync(id, cancellationToken);
    }

    private static Color GetStatusColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "active" => Color.Success,
            "blocked" => Color.Error,
            _ => Color.Default,
        };
    }
}
