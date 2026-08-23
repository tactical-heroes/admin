using System.Globalization;

using TacticalHeroes.Admin.Shared.Navigation;

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
        return RouteUriBuilder.Build(
            Factions,
            ("page", pageNumber == 1
                ? null
                : pageNumber.ToString(CultureInfo.InvariantCulture)),
            ("pageSize", pageSize == 10
                ? null
                : pageSize.ToString(CultureInfo.InvariantCulture)));
    }
}
