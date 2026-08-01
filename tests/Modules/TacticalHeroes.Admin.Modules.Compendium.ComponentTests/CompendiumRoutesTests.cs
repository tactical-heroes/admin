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
}
