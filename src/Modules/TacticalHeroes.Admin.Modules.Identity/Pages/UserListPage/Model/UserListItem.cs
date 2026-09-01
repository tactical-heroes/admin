using MudBlazor;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

public sealed class UserListItem
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public bool IsConfirmed { get; set; }

    public string Status { get; set; } = string.Empty;

    public string StatusDisplayName { get; set; } = string.Empty;

    public Color StatusColor => Status.ToLowerInvariant() switch
    {
        "active" => Color.Success,
        "blocked" => Color.Error,
        _ => Color.Default,
    };
}
