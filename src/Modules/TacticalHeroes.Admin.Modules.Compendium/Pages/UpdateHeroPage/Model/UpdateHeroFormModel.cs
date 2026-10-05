namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Model;

public sealed class UpdateHeroFormModel
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid FactionId { get; set; }

    public int Attack { get; set; }

    public int Defense { get; set; }

    public int MinimumDamage { get; set; }

    public int MaximumDamage { get; set; }

    public double Initiative { get; set; }

    public int Morale { get; set; }

    public int Luck { get; set; }
}
