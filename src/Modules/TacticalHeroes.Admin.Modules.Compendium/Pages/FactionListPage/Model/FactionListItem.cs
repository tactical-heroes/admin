namespace TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Model;

public sealed class FactionListItem
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
