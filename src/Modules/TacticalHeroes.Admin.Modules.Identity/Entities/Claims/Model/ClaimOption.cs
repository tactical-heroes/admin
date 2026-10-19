namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

public sealed class ClaimOption
{
    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? ResourceDisplayName { get; set; }
}
