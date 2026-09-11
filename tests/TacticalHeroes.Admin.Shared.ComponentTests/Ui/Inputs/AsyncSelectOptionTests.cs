using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Inputs;

public sealed class AsyncSelectOptionTests : BunitContext
{
    private const int OptionId = 2;

    private readonly TestOptionsSource _source = new();

    private IRenderedComponent<MudPopoverProvider> Popovers { get; }

    public AsyncSelectOptionTests()
    {
        Services.AddMudServices();
        Services.AddSingleton(_source);
        JSInterop.Mode = JSRuntimeMode.Loose;
        Popovers = Render<MudPopoverProvider>();
    }

    [Fact(DisplayName = "SearchAsync should use configured limits when options are requested")]
    public async Task SearchAsync_Should_UseConfiguredLimits_When_OptionsAreRequested()
    {
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.MaxItems, 1)
            .Add(select => select.MinCharacters, 2));
        var autocomplete = component.FindComponent<MudAutocomplete<int>>();

        var options = await component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("Fi", Xunit.TestContext.Current.CancellationToken)!);

        _source.Limit.ShouldBe(1);
        options.ShouldBe([1]);
        autocomplete.Instance.MaxItems.ShouldBe(1);
        autocomplete.Instance.MinCharacters.ShouldBe(2);
    }

    [Fact(DisplayName = "OnParametersSetAsync should preserve selection when identifier is a string")]
    public async Task OnParametersSetAsync_Should_PreserveSelection_When_IdentifierIsAString()
    {
        var component = Render<StringSelect>(parameters => parameters.Add(select => select.Value, "alpha"));

        await component.WaitForAssertionAsync(() =>
            component.FindComponent<MudAutocomplete<string>>().Find("input").GetAttribute("value").ShouldBe("Alpha"));

        component.Instance.Value.ShouldBe("alpha");
        component.Instance.SelectedOption!.Id.ShouldBe("alpha");
    }

    [Fact(DisplayName = "Render should hide creation link when create route is not configured")]
    public async Task Render_Should_HideCreationLink_When_CreateRouteIsNotConfigured()
    {
        var component = Render<StringSelect>();

        component.FindComponent<MudAutocomplete<string>>().Find("input").Input("missing");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("No options."));

        Popovers.FindAll("a").ShouldBeEmpty();
    }

    [Fact(DisplayName = "OnParametersSetAsync should load once when selected option is absent")]
    public void OnParametersSetAsync_Should_LoadOnce_When_SelectedOptionIsAbsent()
    {
        const int missingId = 3;
        var component = Render<TestSelect>(parameters => parameters.Add(select => select.Value, missingId));

        component.Render(parameters => parameters.Add(select => select.Value, missingId));

        _source.Requests.ShouldBe([null]);
        component.Instance.Value.ShouldBe(missingId);
        component.Instance.SelectedOption.ShouldBeNull();
        component.FindComponent<MudAutocomplete<int>>().Find("input").GetAttribute("value").ShouldBeEmpty();
    }

    [Fact(DisplayName = "LoadAsync should ignore obsolete response when value changes during loading")]
    public async Task LoadAsync_Should_IgnoreObsoleteResponse_When_ValueChangesDuringLoading()
    {
        var firstLoad = new TaskCompletionSource<Result<IReadOnlyList<SelectOption<int>>>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondLoad = new TaskCompletionSource<Result<IReadOnlyList<SelectOption<int>>>>(TaskCreationOptions.RunContinuationsAsynchronously);
        _source.Pending = firstLoad;
        var component = Render<TestSelect>();
        Task first = component.InvokeAsync(() => component.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["Value"] = 1 })));
        await component.WaitForAssertionAsync(() => _source.Requests.Count.ShouldBe(1));
        _source.Pending = secondLoad;
        Task second = component.InvokeAsync(() => component.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["Value"] = 2 })));
        await component.WaitForAssertionAsync(() => _source.Requests.Count.ShouldBe(2));

        firstLoad.SetResult(Result.Success<IReadOnlyList<SelectOption<int>>>([new() { Id = 1, Name = "Old" }]));
        await first;

        component.FindComponent<LoadableContent>().Instance.Loading.ShouldBeTrue();
        component.Instance.SelectedOption.ShouldBeNull();

        secondLoad.SetResult(Result.Success<IReadOnlyList<SelectOption<int>>>([new() { Id = 2, Name = "Current" }]));
        await second;

        component.Instance.SelectedOption!.Id.ShouldBe(2);
        component.FindComponent<MudAutocomplete<int>>().Find("input").GetAttribute("value").ShouldBe("Current");
    }

    [Theory(DisplayName = "SearchAsync should avoid requests when trimmed search is shorter than three characters")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("n")]
    [InlineData("no")]
    [InlineData("   ")]
    [InlineData(" no ")]
    public async Task SearchAsync_Should_AvoidRequests_When_TrimmedSearchIsShorterThanThreeCharacters(string? search)
    {
        var component = Render<TestSelect>();
        var autocomplete = component.FindComponent<MudAutocomplete<int>>();

        var options = await component.InvokeAsync(() => autocomplete.Instance.SearchFunc!(search, Xunit.TestContext.Current.CancellationToken)!);

        options.ShouldBeEmpty();
        _source.Requests.ShouldBeEmpty();
    }

    [Fact(DisplayName = "SearchAsync should cancel pending request when search is cancelled")]
    public async Task SearchAsync_Should_CancelPendingRequest_When_SearchIsCancelled()
    {
        using var source = new CancellationTokenSource();
        _source.Pending = new TaskCompletionSource<Result<IReadOnlyList<SelectOption<int>>>>();
        var component = Render<TestSelect>();
        var autocomplete = component.FindComponent<MudAutocomplete<int>>();

        var search = component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("selected", source.Token)!);
        await component.WaitForAssertionAsync(() => _source.Cancellation.CanBeCanceled.ShouldBeTrue());
        await source.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() => search);
        _source.Cancellation.IsCancellationRequested.ShouldBeTrue();
        component.Markup.ShouldNotContain("Options unavailable.");
    }

    [Fact(DisplayName = "OnParametersSetAsync should load selected option when form has option identifier")]
    public async Task OnParametersSetAsync_Should_LoadSelectedOption_When_FormHasOptionIdentifier()
    {
        var selectedOptionId = OptionId;

        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Value, selectedOptionId)
            .Add(select => select.ValueChanged, value => selectedOptionId = value));

        await component.WaitForAssertionAsync(() =>
        {
            _source.Requests.ShouldBe([null]);
            component.FindComponent<MudAutocomplete<int>>().Find("input").GetAttribute("value").ShouldBe("Selected option");
        });
    }

    [Fact(DisplayName = "OnParametersSetAsync should avoid requests when form has no option")]
    public void OnParametersSetAsync_Should_AvoidRequests_When_FormHasNoOption()
    {
        var selectedOptionId = 0;

        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Value, selectedOptionId)
            .Add(select => select.ValueChanged, value => selectedOptionId = value));

        _source.Requests.ShouldBeEmpty();
        component.FindComponent<MudAutocomplete<int>>().Find("input").GetAttribute("value").ShouldBeEmpty();
    }

    [Fact(DisplayName = "LoadAsync should retry loading when selected option load fails")]
    public async Task LoadAsync_Should_RetryLoading_When_SelectedOptionLoadFails()
    {
        _source.Fail = true;
        var selectedOptionId = OptionId;
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Value, selectedOptionId)
            .Add(select => select.ValueChanged, value => selectedOptionId = value));
        await component.WaitForAssertionAsync(() => component.Markup.ShouldContain("Options unavailable."));
        _source.Fail = false;

        component.FindAll("button").Single(button => button.TextContent.Trim() == "Повторить").Click();

        await component.WaitForAssertionAsync(() =>
        {
            _source.Requests.ShouldBe([null, null]);
            component.FindComponent<MudAutocomplete<int>>().Find("input").GetAttribute("value").ShouldBe("Selected option");
            component.Markup.ShouldNotContain("Options unavailable.");
        });
    }

    [Fact(DisplayName = "SearchAsync should request matching options when text is entered")]
    public async Task SearchAsync_Should_RequestMatchingOptions_When_TextIsEntered()
    {
        var component = Render<TestSelect>();

        component.FindComponent<MudAutocomplete<int>>().Find("input").Input(" sel ");

        await Popovers.WaitForAssertionAsync(() =>
        {
            _source.Requests.ShouldBe(["sel"]);
            Popovers.Markup.ShouldContain("Selected option");
            Popovers.Markup.ShouldNotContain("First option");
        });
    }

    [Fact(DisplayName = "SearchAsync should recover from error when search is repeated")]
    public async Task SearchAsync_Should_RecoverFromError_When_SearchIsRepeated()
    {
        _source.Fail = true;
        var component = Render<TestSelect>();
        var autocomplete = component.FindComponent<MudAutocomplete<int>>();
        await component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("sel", Xunit.TestContext.Current.CancellationToken)!);
        component.Markup.ShouldContain("Options unavailable.");
        _source.Fail = false;

        var options = await component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("selected", Xunit.TestContext.Current.CancellationToken)!);

        _source.Requests.ShouldBe(["sel", "selected"]);
        component.Markup.ShouldNotContain("Options unavailable.");
        options.ShouldBe([OptionId]);
    }

    [Fact(DisplayName = "Render should offer option creation when search has no matches")]
    public async Task Render_Should_OfferOptionCreation_When_SearchHasNoMatches()
    {
        _source.Empty = true;
        var component = Render<TestSelect>();

        component.FindComponent<MudAutocomplete<int>>().Find("input").Input("selected");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("Варианты не найдены."));

        Popovers.Markup.ShouldContain("Варианты не найдены.");
        Popovers.FindAll("a").Select(link => link.GetAttribute("href")).ShouldContain("/options/create");
    }

    [Fact(DisplayName = "SetValueAsync should update option identifier when option is selected")]
    public async Task SetValueAsync_Should_UpdateOptionIdentifier_When_OptionIsSelected()
    {
        var selectedOptionId = 0;
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Value, selectedOptionId)
            .Add(select => select.ValueChanged, value => selectedOptionId = value));
        component.FindComponent<MudAutocomplete<int>>().Find("input").Input("selected");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("Selected option"));

        await Popovers.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == "Selected option").ClickAsync(new MouseEventArgs());

        selectedOptionId.ShouldBe(OptionId);
        component.FindComponent<MudAutocomplete<int>>().Find("input").GetAttribute("value").ShouldBe("Selected option");
    }

    [Fact(DisplayName = "SetValueAsync should clear option identifier when selection is cleared")]
    public async Task SetValueAsync_Should_ClearOptionIdentifier_When_SelectionIsCleared()
    {
        var selectedOptionId = OptionId;
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Value, selectedOptionId)
            .Add(select => select.ValueChanged, value => selectedOptionId = value));
        component.WaitForElement("input");

        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<int>>().Instance.ClearAsync());

        selectedOptionId.ShouldBe(0);
        component.FindComponent<MudAutocomplete<int>>().Find("input").GetAttribute("value").ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetName should preserve selected label when other options are searched")]
    public async Task GetName_Should_PreserveSelectedLabel_When_OtherOptionsAreSearched()
    {
        var selectedOptionId = OptionId;
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Value, selectedOptionId)
            .Add(select => select.ValueChanged, value => selectedOptionId = value));
        component.FindComponent<MudAutocomplete<int>>().Find("input").Input("First option");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("First option"));

        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<int>>().Instance.CloseMenuAsync());

        selectedOptionId.ShouldBe(OptionId);
        component.FindComponent<MudAutocomplete<int>>().Find("input").GetAttribute("value").ShouldBe("Selected option");
    }

    private sealed class StringSelect() : AsyncSelectOption<string, SelectOption<string>>(
        (search, _, _) => Task.FromResult(Result.Success<IReadOnlyList<SelectOption<string>>>(
            search is null ? [new() { Id = "alpha", Name = "Alpha" }] : [])),
        "Option",
        "No options.");

    private sealed class TestSelect(TestOptionsSource source)
        : AsyncSelectOption<int, SelectOption<int>>(source.LoadAsync, "Вариант", "Варианты не найдены.", "/options/create", "Создать вариант");

    private sealed class TestOptionsSource
    {
        public List<string?> Requests { get; } = [];

        public int Limit { get; private set; }

        public bool Fail { get; set; }

        public bool Empty { get; set; }

        public TaskCompletionSource<Result<IReadOnlyList<SelectOption<int>>>>? Pending { get; set; }

        public CancellationToken Cancellation { get; private set; }

        public async Task<Result<IReadOnlyList<SelectOption<int>>>> LoadAsync(
            string? search, int limit, CancellationToken cancellationToken)
        {
            Requests.Add(search);
            Limit = limit;
            Cancellation = cancellationToken;
            if (Pending is not null)
            {
                return await Pending.Task.WaitAsync(cancellationToken);
            }

            if (Fail)
            {
                return Result.Failure<IReadOnlyList<SelectOption<int>>>(Error.Failure("Options unavailable."));
            }

            SelectOption<int>[] items =
            [
                new() { Id = 1, Name = "First option" },
                new() { Id = OptionId, Name = "Selected option" }
            ];
            IReadOnlyList<SelectOption<int>> options = Empty
                ? []
                : items.Where(option => search is null || option.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).Take(limit).ToArray();
            return Result.Success(options);
        }
    }
}
