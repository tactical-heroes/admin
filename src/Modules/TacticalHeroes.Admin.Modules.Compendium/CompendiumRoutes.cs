namespace TacticalHeroes.Admin.Modules.Compendium;

public static class CompendiumRoutes
{
    public const string Factions = "/factions";

    public const string CreateFaction = $"{Factions}/new";

    public const string FactionTemplate = $"{Factions}/{{Id:guid}}";

    public const string Heroes = "/heroes";

    public const string CreateHero = $"{Heroes}/new";

    public const string HeroTemplate = $"{Heroes}/{{Id:guid}}";

    public const string Units = "/units";

    public const string CreateUnit = $"{Units}/new";

    public const string UnitTemplate = $"{Units}/{{Id:guid}}";

    public static string Faction(Guid id)
    {
        return $"{Factions}/{id:D}";
    }

    public static string Hero(Guid id)
    {
        return $"{Heroes}/{id:D}";
    }

    public static string Unit(Guid id)
    {
        return $"{Units}/{id:D}";
    }
}
