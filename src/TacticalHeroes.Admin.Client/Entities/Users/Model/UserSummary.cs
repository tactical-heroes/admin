namespace TacticalHeroes.Admin.Client.Entities.Users.Model;

public sealed record UserSummary(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName);
