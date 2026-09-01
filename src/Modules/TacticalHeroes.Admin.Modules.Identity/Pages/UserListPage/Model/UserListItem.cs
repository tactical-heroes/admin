using MudBlazor;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;

public sealed class UserListItem(
    Guid id,
    string email,
    string userName,
    bool isConfirmed,
    string status,
    string statusDisplayName)
{
    public Guid Id { get; } = id;

    public string Email { get; } = email;

    public string UserName { get; } = userName;

    public bool IsConfirmed { get; } = isConfirmed;

    public string Status { get; } = status;

    public string StatusDisplayName { get; } = statusDisplayName;

    public Color StatusColor => Status.ToLowerInvariant() switch
    {
        "active" => Color.Success,
        "blocked" => Color.Error,
        _ => Color.Default,
    };
}
