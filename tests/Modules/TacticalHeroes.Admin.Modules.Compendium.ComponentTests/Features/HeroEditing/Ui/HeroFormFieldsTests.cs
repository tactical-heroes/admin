using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Model;
using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Features.HeroEditing.Ui;

public sealed class HeroFormFieldsTests : HeroFormTestContext
{
    [Fact(DisplayName = "SearchFactionsAsync should cancel pending request when search is cancelled")]
    public async Task SearchFactionsAsync_Should_CancelPendingRequest_When_SearchIsCancelled()
    {
        using var source = new CancellationTokenSource();
        Handler.PendingFactionSearch = new TaskCompletionSource<HttpResponseMessage>();
        var component = Render<HeroFormFields>();
        var autocomplete = component.FindComponent<MudAutocomplete<Guid>>();

        var search = component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("north", source.Token)!);
        component.WaitForAssertion(() => Handler.FactionSearchCancellation.CanBeCanceled.ShouldBeTrue());
        await source.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => search);
        Handler.FactionSearchCancellation.IsCancellationRequested.ShouldBeTrue();
        component.Markup.ShouldNotContain("Factions unavailable.");
    }

    [Fact(DisplayName = "OnParametersSetAsync should load selected faction when form has faction identifier")]
    public void OnParametersSetAsync_Should_LoadSelectedFaction_When_FormHasFactionIdentifier()
    {
        var model = new HeroFormModel { FactionId = FactionId };

        var component = Render<HeroFormFields>(parameters => parameters.Add(fields => fields.Model, model));

        component.WaitForAssertion(() =>
        {
            Handler.FactionDetailRequests.ShouldBe(1);
            Handler.FactionRequests.ShouldBeEmpty();
            component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
        });
    }

    [Fact(DisplayName = "OnParametersSetAsync should avoid requests when form has no faction")]
    public void OnParametersSetAsync_Should_AvoidRequests_When_FormHasNoFaction()
    {
        var model = new HeroFormModel();

        var component = Render<HeroFormFields>(parameters => parameters.Add(fields => fields.Model, model));

        Handler.FactionDetailRequests.ShouldBe(0);
        Handler.FactionRequests.ShouldBeEmpty();
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBeEmpty();
    }

    [Fact(DisplayName = "LoadSelectedFactionAsync should retry loading when selected faction load fails")]
    public void LoadSelectedFactionAsync_Should_RetryLoading_When_SelectedFactionLoadFails()
    {
        Handler.FailFactionLoad = true;
        var model = new HeroFormModel { FactionId = FactionId };
        var component = Render<HeroFormFields>(parameters => parameters.Add(fields => fields.Model, model));
        component.WaitForAssertion(() => component.Markup.ShouldContain("Faction unavailable."));
        Handler.FailFactionLoad = false;

        component.FindAll("button").Single(button => button.TextContent.Trim() == "Повторить").Click();

        component.WaitForAssertion(() =>
        {
            Handler.FactionDetailRequests.ShouldBe(2);
            component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
            component.Markup.ShouldNotContain("Faction unavailable.");
        });
    }

    [Fact(DisplayName = "SearchFactionsAsync should request matching options when text is entered")]
    public void SearchFactionsAsync_Should_RequestMatchingOptions_When_TextIsEntered()
    {
        var component = Render<HeroFormFields>();

        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("north");

        component.WaitForAssertion(() =>
        {
            Handler.FactionRequests.ShouldBe(["north"]);
            Popovers.Markup.ShouldContain("Northern Alliance");
            Popovers.Markup.ShouldNotContain("Faction 1");
        });
    }

    [Fact(DisplayName = "SearchFactionsAsync should recover from error when search is repeated")]
    public void SearchFactionsAsync_Should_RecoverFromError_When_SearchIsRepeated()
    {
        Handler.FailFactionSearch = true;
        var component = Render<HeroFormFields>();
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("nor");
        component.WaitForAssertion(() => component.Markup.ShouldContain("Factions unavailable."));
        Handler.FailFactionSearch = false;

        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("north");

        component.WaitForAssertion(() =>
        {
            Handler.FactionRequests.ShouldBe(["nor", "north"]);
            component.Markup.ShouldNotContain("Factions unavailable.");
            Popovers.Markup.ShouldContain("Northern Alliance");
        });
    }

    [Fact(DisplayName = "Render should offer faction creation when search has no matches")]
    public async Task Render_Should_OfferFactionCreation_When_SearchHasNoMatches()
    {
        Handler.EmptyFactions = true;
        var component = Render<HeroFormFields>();

        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<Guid>>().Instance.OpenMenuAsync());

        Popovers.Markup.ShouldContain("Фракции не найдены.");
        Popovers.FindAll("a").Select(link => link.GetAttribute("href")).ShouldContain(CompendiumRoutes.CreateFaction);
    }

    [Fact(DisplayName = "SetFaction should update faction identifier when option is selected")]
    public async Task SetFaction_Should_UpdateFactionIdentifier_When_OptionIsSelected()
    {
        var model = new HeroFormModel();
        var component = Render<HeroFormFields>(parameters => parameters.Add(fields => fields.Model, model));
        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<Guid>>().Instance.OpenMenuAsync());

        Popovers.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == "Northern Alliance").Click();

        model.FactionId.ShouldBe(FactionId);
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
    }

    [Fact(DisplayName = "SetFaction should clear faction identifier when selection is cleared")]
    public async Task SetFaction_Should_ClearFactionIdentifier_When_SelectionIsCleared()
    {
        var model = new HeroFormModel { FactionId = FactionId };
        var component = Render<HeroFormFields>(parameters => parameters.Add(fields => fields.Model, model));
        component.WaitForElement("input");

        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<Guid>>().Instance.ClearAsync());

        model.FactionId.ShouldBe(Guid.Empty);
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetFactionName should preserve selected label when other options are searched")]
    public async Task GetFactionName_Should_PreserveSelectedLabel_When_OtherOptionsAreSearched()
    {
        var model = new HeroFormModel { FactionId = FactionId };
        var component = Render<HeroFormFields>(parameters => parameters.Add(fields => fields.Model, model));
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("Faction 1");
        component.WaitForAssertion(() => Handler.FactionRequests.ShouldContain("Faction 1"));

        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<Guid>>().Instance.CloseMenuAsync());

        model.FactionId.ShouldBe(FactionId);
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
    }
}
