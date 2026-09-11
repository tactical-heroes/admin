using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Entities.Factions.Ui;

public sealed class FactionSelectTests : HeroFormTestContext
{
    [Fact(DisplayName = "Render should load selected faction when form has faction identifier")]
    public async Task Render_Should_LoadSelectedFaction_When_FormHasFactionIdentifier()
    {
        var selectedFactionId = FactionId;

        var component = Render<FactionSelect>(parameters => parameters
            .Add(select => select.Value, selectedFactionId)
            .Add(select => select.ValueChanged, value => selectedFactionId = value));

        await component.WaitForAssertionAsync(() =>
        {
            Handler.FactionRequests.ShouldBe([null]);
            component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
        });
    }

    [Fact(DisplayName = "Render should request matching options when text is entered")]
    public async Task Render_Should_RequestMatchingOptions_When_TextIsEntered()
    {
        var component = Render<FactionSelect>();

        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input(" nor ");

        await Popovers.WaitForAssertionAsync(() =>
        {
            Handler.FactionRequests.ShouldBe(["nor"]);
            Popovers.Markup.ShouldContain("Northern Alliance");
            Popovers.Markup.ShouldNotContain("Faction 1");
        });
    }

    [Fact(DisplayName = "Render should offer faction creation when search has no matches")]
    public async Task Render_Should_OfferFactionCreation_When_SearchHasNoMatches()
    {
        Handler.EmptyFactions = true;
        var component = Render<FactionSelect>();

        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("north");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("Фракции не найдены."));

        Popovers.Markup.ShouldContain("Фракции не найдены.");
        Popovers.FindAll("a").Select(link => link.GetAttribute("href")).ShouldContain(CompendiumRoutes.CreateFaction);
    }
}
