namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests;

public sealed class CompendiumRoutesTests
{
    [Fact(DisplayName = "Hero should build route when identifier is provided")]
    public void Hero_Should_BuildRoute_When_IdentifierIsProvided()
    {
        var id = Guid.Parse("0dd33d34-7b22-4f9a-91fe-d1842b7776c5");

        string route = CompendiumRoutes.Hero(id);

        route.ShouldBe("/heroes/0dd33d34-7b22-4f9a-91fe-d1842b7776c5");
    }

    [Fact(DisplayName = "Faction should build route when identifier is provided")]
    public void Faction_Should_BuildRoute_When_IdentifierIsProvided()
    {
        var id = Guid.Parse("0dd33d34-7b22-4f9a-91fe-d1842b7776c5");

        string route = CompendiumRoutes.Faction(id);

        route.ShouldBe("/factions/0dd33d34-7b22-4f9a-91fe-d1842b7776c5");
    }
}
