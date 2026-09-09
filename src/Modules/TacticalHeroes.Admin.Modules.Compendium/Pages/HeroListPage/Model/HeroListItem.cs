namespace TacticalHeroes.Admin.Modules.Compendium.Pages.HeroListPage.Model;

public sealed class HeroListItem
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid FactionId { get; set; }

    public string FactionName { get; set; } = string.Empty;
}
