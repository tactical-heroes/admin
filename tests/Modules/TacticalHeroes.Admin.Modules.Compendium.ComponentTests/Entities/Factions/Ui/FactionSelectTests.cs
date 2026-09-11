using Microsoft.AspNetCore.Components.Web;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Entities.Factions.Ui;

public sealed class FactionSelectTests : HeroFormTestContext
{
    [Theory(DisplayName = "SearchFactionsAsync should avoid requests when trimmed search is shorter than three characters")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("n")]
    [InlineData("no")]
    [InlineData("   ")]
    [InlineData(" no ")]
    public async Task SearchFactionsAsync_Should_AvoidRequests_When_TrimmedSearchIsShorterThanThreeCharacters(string? search)
    {
        var component = Render<FactionSelect>();
        var autocomplete = component.FindComponent<MudAutocomplete<Guid>>();

        var options = await component.InvokeAsync(() => autocomplete.Instance.SearchFunc!(search, Xunit.TestContext.Current.CancellationToken)!);

        options.ShouldBeEmpty();
        Handler.FactionRequests.ShouldBeEmpty();
    }

    [Fact(DisplayName = "SearchFactionsAsync should cancel pending request when search is cancelled")]
    public async Task SearchFactionsAsync_Should_CancelPendingRequest_When_SearchIsCancelled()
    {
        using var source = new CancellationTokenSource();
        Handler.PendingFactionSearch = new TaskCompletionSource<HttpResponseMessage>();
        var component = Render<FactionSelect>();
        var autocomplete = component.FindComponent<MudAutocomplete<Guid>>();

        var search = component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("north", source.Token)!);
        await component.WaitForAssertionAsync(() => Handler.FactionSearchCancellation.CanBeCanceled.ShouldBeTrue());
        await source.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => search);
        Handler.FactionSearchCancellation.IsCancellationRequested.ShouldBeTrue();
        component.Markup.ShouldNotContain("Factions unavailable.");
    }

    [Fact(DisplayName = "OnParametersSetAsync should load selected faction when form has faction identifier")]
    public async Task OnParametersSetAsync_Should_LoadSelectedFaction_When_FormHasFactionIdentifier()
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

    [Fact(DisplayName = "OnParametersSetAsync should avoid requests when form has no faction")]
    public void OnParametersSetAsync_Should_AvoidRequests_When_FormHasNoFaction()
    {
        var selectedFactionId = Guid.Empty;

        var component = Render<FactionSelect>(parameters => parameters
            .Add(select => select.Value, selectedFactionId)
            .Add(select => select.ValueChanged, value => selectedFactionId = value));

        Handler.FactionRequests.ShouldBeEmpty();
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBeEmpty();
    }

    [Fact(DisplayName = "LoadSelectedFactionAsync should retry loading when selected faction load fails")]
    public async Task LoadSelectedFactionAsync_Should_RetryLoading_When_SelectedFactionLoadFails()
    {
        Handler.FailFactionSearch = true;
        var selectedFactionId = FactionId;
        var component = Render<FactionSelect>(parameters => parameters
            .Add(select => select.Value, selectedFactionId)
            .Add(select => select.ValueChanged, value => selectedFactionId = value));
        await component.WaitForAssertionAsync(() => component.Markup.ShouldContain("Factions unavailable."));
        Handler.FailFactionSearch = false;

        component.FindAll("button").Single(button => button.TextContent.Trim() == "Повторить").Click();

        await component.WaitForAssertionAsync(() =>
        {
            Handler.FactionRequests.ShouldBe([null, null]);
            component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
            component.Markup.ShouldNotContain("Factions unavailable.");
        });
    }

    [Fact(DisplayName = "SearchFactionsAsync should request matching options when text is entered")]
    public async Task SearchFactionsAsync_Should_RequestMatchingOptions_When_TextIsEntered()
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

    [Fact(DisplayName = "SearchFactionsAsync should recover from error when search is repeated")]
    public async Task SearchFactionsAsync_Should_RecoverFromError_When_SearchIsRepeated()
    {
        Handler.FailFactionSearch = true;
        var component = Render<FactionSelect>();
        var autocomplete = component.FindComponent<MudAutocomplete<Guid>>();
        await component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("nor", Xunit.TestContext.Current.CancellationToken)!);
        component.Markup.ShouldContain("Factions unavailable.");
        Handler.FailFactionSearch = false;

        var options = await component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("north", Xunit.TestContext.Current.CancellationToken)!);

        Handler.FactionRequests.ShouldBe(["nor", "north"]);
        component.Markup.ShouldNotContain("Factions unavailable.");
        options.ShouldBe([FactionId]);
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

    [Fact(DisplayName = "SetFactionAsync should update faction identifier when option is selected")]
    public async Task SetFactionAsync_Should_UpdateFactionIdentifier_When_OptionIsSelected()
    {
        var selectedFactionId = Guid.Empty;
        var component = Render<FactionSelect>(parameters => parameters
            .Add(select => select.Value, selectedFactionId)
            .Add(select => select.ValueChanged, value => selectedFactionId = value));
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("north");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("Northern Alliance"));

        await Popovers.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == "Northern Alliance").ClickAsync(new MouseEventArgs());

        selectedFactionId.ShouldBe(FactionId);
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
    }

    [Fact(DisplayName = "SetFactionAsync should clear faction identifier when selection is cleared")]
    public async Task SetFactionAsync_Should_ClearFactionIdentifier_When_SelectionIsCleared()
    {
        var selectedFactionId = FactionId;
        var component = Render<FactionSelect>(parameters => parameters
            .Add(select => select.Value, selectedFactionId)
            .Add(select => select.ValueChanged, value => selectedFactionId = value));
        component.WaitForElement("input");

        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<Guid>>().Instance.ClearAsync());

        selectedFactionId.ShouldBe(Guid.Empty);
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetFactionName should preserve selected label when other options are searched")]
    public async Task GetFactionName_Should_PreserveSelectedLabel_When_OtherOptionsAreSearched()
    {
        var selectedFactionId = FactionId;
        var component = Render<FactionSelect>(parameters => parameters
            .Add(select => select.Value, selectedFactionId)
            .Add(select => select.ValueChanged, value => selectedFactionId = value));
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("Faction 1");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("Faction 1"));

        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<Guid>>().Instance.CloseMenuAsync());

        selectedFactionId.ShouldBe(FactionId);
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
    }
}
