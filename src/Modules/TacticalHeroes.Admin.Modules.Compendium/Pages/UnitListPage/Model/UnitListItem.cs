namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UnitListPage.Model;

public sealed class UnitListItem
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid FactionId { get; set; }

    public string FactionName { get; set; } = string.Empty;
}
