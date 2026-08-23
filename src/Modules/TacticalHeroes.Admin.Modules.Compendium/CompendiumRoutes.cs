namespace TacticalHeroes.Admin.Modules.Compendium;

public static class CompendiumRoutes
{
    public const string Factions = "/factions";

    public const string CreateFaction = "/factions/new";

    public const string FactionTemplate = "/factions/{Id:guid}";

    public static string Faction(Guid id)
    {
        return $"{Factions}/{id:D}";
    }
}
