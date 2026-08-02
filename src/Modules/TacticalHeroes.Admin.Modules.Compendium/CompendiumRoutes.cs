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

    public static string FactionsPage(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                pageNumber,
                "Page number must be greater than zero.");
        }

        ValidatePageSize(pageSize);

        var query = new List<string>();

        if (pageNumber != 1)
        {
            query.Add($"page={pageNumber.ToString(CultureInfo.InvariantCulture)}");
        }

        if (pageSize != 10)
        {
            query.Add($"pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}");
        }

        return query.Count == 0
            ? Factions
            : $"{Factions}?{string.Join('&', query)}";
    }

    private static void ValidatePageSize(int pageSize)
    {
        if (pageSize is not (10 or 25 or 50 or 100))
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                pageSize,
                "Page size must be 10, 25, 50, or 100.");
        }
    }
}
