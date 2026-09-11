namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Model;

public sealed class UpdateUnitFormModel
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid FactionId { get; set; }

    public int Attack { get; set; }

    public int Defense { get; set; }

    public int Health { get; set; } = 1;

    public int MinimumDamage { get; set; }

    public int MaximumDamage { get; set; }

    public double Initiative { get; set; }

    public int Speed { get; set; }

    public int? Shots { get; set; }

    public int? RangedAttackRange { get; set; }

    public int Morale { get; set; }

    public int Luck { get; set; }
}
