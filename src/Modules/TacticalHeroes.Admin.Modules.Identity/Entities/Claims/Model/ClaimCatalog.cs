namespace TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;

public sealed class ClaimCatalog
{
    public string Type { get; set; } = string.Empty;

    public IReadOnlyList<ClaimOption> Items { get; set; } = [];
}
