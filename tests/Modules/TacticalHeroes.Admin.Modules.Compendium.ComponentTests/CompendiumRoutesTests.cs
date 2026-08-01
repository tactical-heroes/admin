namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests;

public sealed class CompendiumRoutesTests
{
    [Fact(DisplayName = "Builds faction route from identifier")]
    public void Faction_Should_BuildRoute_When_IdentifierIsProvided()
    {
        var id = Guid.Parse("0dd33d34-7b22-4f9a-91fe-d1842b7776c5");

        string route = CompendiumRoutes.Faction(id);

        route.ShouldBe("/factions/0dd33d34-7b22-4f9a-91fe-d1842b7776c5");
    }

    [Fact(DisplayName = "Builds factions route from page")]
    public void FactionsPage_Should_IncludePage_When_PageIsNotFirst()
    {
        string route = CompendiumRoutes.FactionsPage(pageNumber: 4);

        route.ShouldBe("/factions?page=4");
    }

    [Fact(DisplayName = "Builds factions route from page size")]
    public void FactionsPage_Should_IncludePageSize_When_PageSizeIsNotDefault()
    {
        string route = CompendiumRoutes.FactionsPage(pageNumber: 2, pageSize: 50);

        route.ShouldBe("/factions?page=2&pageSize=50");
    }
}
