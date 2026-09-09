using Microsoft.AspNetCore.Components.Web;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Model;
using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Features.HeroEditing.Ui;

public sealed class HeroFormFieldsTests : HeroFormTestContext
{
    [Fact(DisplayName = "OnInitializedAsync should load every faction page when catalog exceeds page size")]
    public void OnInitializedAsync_Should_LoadEveryFactionPage_When_CatalogExceedsPageSize()
    {
        Handler.MultipleFactionPages = true;
        var model = new HeroFormModel { FactionId = FactionId };

        var component = Render<HeroFormFields>(parameters => parameters.Add(fields => fields.Model, model));

        component.WaitForAssertion(() =>
        {
            Handler.FactionRequests.ShouldBe([1, 2]);
            component.FindComponents<MudSelectItem<Guid>>().Count.ShouldBe(101);
            component.FindComponent<MudSelect<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
        });
    }

    [Fact(DisplayName = "LoadFactionsAsync should retry every page when later faction page fails")]
    public void LoadFactionsAsync_Should_RetryEveryPage_When_LaterFactionPageFails()
    {
        Handler.MultipleFactionPages = true;
        Handler.FailingFactionPage = 2;
        var component = Render<HeroFormFields>();
        component.WaitForAssertion(() => component.Markup.ShouldContain("Factions unavailable."));
        Handler.FailingFactionPage = null;

        component.FindAll("button").Single(button => button.TextContent.Trim() == "Повторить").Click();

        component.WaitForAssertion(() =>
        {
            Handler.FactionRequests.ShouldBe([1, 2, 1, 2]);
            component.FindComponents<MudSelectItem<Guid>>().Count.ShouldBe(101);
            component.Markup.ShouldNotContain("Factions unavailable.");
        });
    }

    [Fact(DisplayName = "Render should offer faction creation when faction catalog is empty")]
    public void Render_Should_OfferFactionCreation_When_FactionCatalogIsEmpty()
    {
        Handler.EmptyFactions = true;

        var component = Render<HeroFormFields>();

        component.WaitForAssertion(() =>
        {
            component.FindAll("a").Select(link => link.GetAttribute("href")).ShouldContain(CompendiumRoutes.CreateFaction);
            component.FindComponents<MudSelectItem<Guid>>().ShouldBeEmpty();
        });
    }

    [Fact(DisplayName = "Render should update faction identifier when option is selected")]
    public void Render_Should_UpdateFactionIdentifier_When_OptionIsSelected()
    {
        var model = new HeroFormModel();
        var component = Render<HeroFormFields>(parameters => parameters.Add(fields => fields.Model, model));

        component.FindComponent<MudSelect<Guid>>().Find(".mud-input-control").MouseDown(new MouseEventArgs());
        Popovers.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == "Northern Alliance").Click();

        model.FactionId.ShouldBe(FactionId);
        component.FindComponent<MudSelect<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
    }
}
