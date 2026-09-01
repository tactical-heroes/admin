namespace TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Model;

public sealed class FactionListItem(
    Guid id,
    string name,
    string description)
{
    public Guid Id { get; } = id;

    public string Name { get; } = name;

    public string Description { get; } = description;
}
