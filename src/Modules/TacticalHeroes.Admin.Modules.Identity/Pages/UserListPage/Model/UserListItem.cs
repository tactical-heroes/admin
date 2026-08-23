using MudBlazor;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

public sealed record UserListItem(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName)
{
    public Color StatusColor => Status.ToLowerInvariant() switch
    {
        "active" => Color.Success,
        "blocked" => Color.Error,
        _ => Color.Default,
    };
}
