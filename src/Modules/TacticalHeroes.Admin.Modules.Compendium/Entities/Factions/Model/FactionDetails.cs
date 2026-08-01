namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;

public sealed class FactionDetails
{
    public Guid? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
