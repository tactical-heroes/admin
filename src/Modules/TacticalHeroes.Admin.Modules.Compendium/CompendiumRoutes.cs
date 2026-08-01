using System.Globalization;

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

    public static string FactionsPage(int pageNumber = 1)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "Page number must be greater than zero.");
        }

        return pageNumber == 1
            ? Factions
            : $"{Factions}?page={pageNumber.ToString(CultureInfo.InvariantCulture)}";
    }
}
