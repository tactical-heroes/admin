using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;
using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Inputs;

public sealed class AsyncMultiSelectOptionTests : BunitContext
{
    private const int OptionId = 2;

    private readonly TestOptionsSource _source = new();

    private IRenderedComponent<MudPopoverProvider> Popovers { get; }

    public AsyncMultiSelectOptionTests()
    {
        Services.AddMudServices();
        Services.AddSingleton(_source);
        JSInterop.Mode = JSRuntimeMode.Loose;
        Popovers = Render<MudPopoverProvider>();
    }

    [Fact(DisplayName = "OnParametersSetAsync should reuse loaded catalog when selection changes")]
    public void OnParametersSetAsync_Should_ReuseLoadedCatalog_When_SelectionChanges()
    {
        var component = Render<TestSelect>();

        component.Render(parameters => parameters.Add(select => select.Values, [OptionId]));

        _source.Requests.ShouldBe(1);
        component.Instance.Items!.Count.ShouldBe(2);
        component.Find(".mud-chip-content").TextContent.Trim().ShouldBe("Selected option");
    }

    [Fact(DisplayName = "LoadAsync should preserve selection and retry only on request when catalog loading fails")]
    public async Task LoadAsync_Should_PreserveSelectionAndRetryOnlyOnRequest_When_CatalogLoadingFails()
    {
        _source.Fail = true;
        var component = Render<TestSelect>(parameters => parameters.Add(select => select.Values, [OptionId]));
        component.Render(parameters => parameters.Add(select => select.Values, [OptionId, 3]));
        _source.Requests.ShouldBe(1);
        component.Instance.Values.ShouldBe([OptionId, 3]);
        component.Markup.ShouldContain("Options unavailable.");
        _source.Fail = false;

        await component.FindAll("button").Single(button => button.TextContent.Trim() == "Повторить")
            .ClickAsync(new MouseEventArgs());

        _source.Requests.ShouldBe(2);
        component.Instance.Values.ShouldBe([OptionId, 3]);
        component.FindAll(".mud-chip-content").Select(chip => chip.TextContent.Trim())
            .ShouldBe(["Selected option", "Недоступный вариант (3)"]);
    }

    [Fact(DisplayName = "ToggleValueAsync should preserve other selections when an option is toggled")]
    public async Task ToggleValueAsync_Should_PreserveOtherSelections_When_AnOptionIsToggled()
    {
        List<int> values = [1];
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Values, values)
            .Add(select => select.ValuesChanged, selected => values = selected));
        await component.Find("fieldset").ClickAsync(new MouseEventArgs());

        await Popovers.FindAll("[role=menuitemcheckbox]").Single(item => item.TextContent.Trim() == "Selected option")
            .ClickAsync(new MouseEventArgs());

        values.ShouldBe([1, OptionId]);
        component.FindAll(".mud-chip-content").Select(chip => chip.TextContent.Trim()).ShouldBe(["First option", "Selected option"]);
        await Popovers.FindAll("[role=menuitemcheckbox]").Single(item => item.TextContent.Trim() == "First option")
            .ClickAsync(new MouseEventArgs());
        values.ShouldBe([OptionId]);
        component.Find(".mud-chip-content").TextContent.Trim().ShouldBe("Selected option");
        component.FindComponents<MudAutocomplete<int>>().ShouldBeEmpty();
        component.FindComponents<MudChip<int>>().Count.ShouldBe(1);
        component.Find(".mud-menu-activator").GetAttribute("aria-expanded").ShouldBe("true");
    }

    [Fact(DisplayName = "GetSelectedName should retain missing selections when catalog does not contain an identifier")]
    public async Task GetSelectedName_Should_RetainMissingSelections_When_CatalogDoesNotContainAnIdentifier()
    {
        const int missingId = 3;
        List<int> values = [OptionId, missingId];
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Values, values)
            .Add(select => select.ValuesChanged, selected => values = selected));
        component.FindAll(".mud-chip-content").Select(chip => chip.TextContent.Trim()).ShouldBe(["Selected option", "Недоступный вариант (3)"]);
        await component.Find("fieldset").ClickAsync(new MouseEventArgs());

        await Popovers.FindAll("[role=menuitemcheckbox]").Single(item => item.TextContent.Trim() == "Недоступный вариант (3)")
            .ClickAsync(new MouseEventArgs());

        values.ShouldBe([OptionId]);
        component.Find(".mud-chip-content").TextContent.Trim().ShouldBe("Selected option");
        _source.Requests.ShouldBe(1);
    }

    [Fact(DisplayName = "LoadAsync should keep current selections when values change during loading")]
    public async Task LoadAsync_Should_KeepCurrentSelections_When_ValuesChangeDuringLoading()
    {
        _source.Pending = new TaskCompletionSource<Result<IReadOnlyList<SelectOption<int>>>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Values, [OptionId]));
        await component.WaitForAssertionAsync(() => _source.Requests.ShouldBe(1));

        await component.InvokeAsync(() => component.Instance.SetParametersAsync(
            ParameterView.FromDictionary(new Dictionary<string, object?> { ["Values"] = new List<int>() })));
        _source.Pending.SetResult(Result.Success<IReadOnlyList<SelectOption<int>>>([new() { Id = OptionId, Name = "Old" }]));

        await component.WaitForAssertionAsync(() =>
        {
            component.Instance.Values.ShouldBeEmpty();
            component.FindComponents<MudChip<int>>().ShouldBeEmpty();
            component.Find(".multi-select-placeholder").TextContent.ShouldBe("Выберите варианты");
        });
        _source.Requests.ShouldBe(1);
    }

    [Fact(DisplayName = "GetOptionName should format selected labels when display names are customized")]
    public void GetOptionName_Should_FormatSelectedLabels_When_DisplayNamesAreCustomized()
    {
        var component = Render<FormattedSelect>(parameters => parameters.Add(select => select.Values, [OptionId]));

        component.Find(".mud-chip-content").TextContent.Trim().ShouldBe("Name: Selected option");
    }

    [Fact(DisplayName = "SetValuesAsync should clear selection when all options are deselected")]
    public async Task SetValuesAsync_Should_ClearSelection_When_AllOptionsAreDeselected()
    {
        List<int> values = [1, OptionId];
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Values, values)
            .Add(select => select.ValuesChanged, selected => values = selected));

        foreach (var chip in component.FindComponents<MudChip<int>>())
        {
            await chip.Find("button").ClickAsync(new MouseEventArgs());
        }

        values.ShouldBeEmpty();
        component.FindComponents<MudChip<int>>().ShouldBeEmpty();
    }

    [Fact(DisplayName = "ToggleSelected should expand selected chips without changing values when more than four options are selected")]
    public async Task ToggleSelected_Should_ExpandSelectedChipsWithoutChangingValues_When_MoreThanFourOptionsAreSelected()
    {
        List<int> values = [1, 2, 3, 4, 5, 6, 7];
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Values, values));
        component.FindComponents<MudChip<int>>().Count.ShouldBe(4);
        component.Find(".multi-select-more").TextContent.Trim().ShouldBe("Ещё 3");

        await component.Find(".multi-select-more").ClickAsync(new MouseEventArgs());

        component.FindComponents<MudChip<int>>().Count.ShouldBe(7);
        component.Find(".mud-menu-activator").GetAttribute("aria-expanded").ShouldBe("false");
        component.Instance.Values.ShouldBe(values);
        await component.Find(".multi-select-more").ClickAsync(new MouseEventArgs());
        component.FindComponents<MudChip<int>>().Count.ShouldBe(4);
    }

    [Fact(DisplayName = "Render should show empty catalog text when no options are available")]
    public async Task Render_Should_ShowEmptyCatalogText_When_NoOptionsAreAvailable()
    {
        _source.Empty = true;
        var component = Render<TestSelect>();

        await component.Find("fieldset").ClickAsync(new MouseEventArgs());

        component.Find(".multi-select-placeholder").TextContent.ShouldBe("Выберите варианты");
        Popovers.Markup.ShouldContain("Варианты не найдены.");
        Popovers.FindAll("[role=menuitemcheckbox]").ShouldBeEmpty();
    }

    [Fact(DisplayName = "Render should describe validation errors when multi selection is invalid")]
    public void Render_Should_DescribeValidationErrors_When_MultiSelectionIsInvalid()
    {
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Values, [OptionId])
            .Add(select => select.Error, true)
            .Add(select => select.ErrorText, "Invalid selection."));

        var field = component.Find("fieldset");

        field.GetAttribute("aria-invalid").ShouldBe("true");
        component.Find($"#{field.GetAttribute("aria-describedby")}").TextContent.ShouldContain("Invalid selection.");
        component.Find("[role=alert]").TextContent.ShouldContain("Invalid selection.");
        component.Instance.Values.ShouldBe([OptionId]);
    }

    [Fact(DisplayName = "RemoveValueAsync should remove missing chip without opening menu when its close action is used")]
    public async Task RemoveValueAsync_Should_RemoveMissingChipWithoutOpeningMenu_When_ItsCloseActionIsUsed()
    {
        List<int> values = [OptionId, 3, 3];
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Values, values)
            .Add(select => select.ValuesChanged, selected => values = selected));
        var missing = component.FindComponents<MudChip<int>>().Single(chip => chip.Instance.Value == 3);
        missing.Instance.Color.ShouldBe(Color.Warning);

        await missing.Find(".mud-chip-close-button").ClickAsync(new MouseEventArgs());

        values.ShouldBe([OptionId]);
        component.Find(".mud-menu-activator").GetAttribute("aria-expanded").ShouldBe("false");
        component.Markup.ShouldNotContain("Недоступные варианты сохранены");
    }

    [Fact(DisplayName = "IsMissing should highlight only unavailable values when selected identifiers are absent from catalog")]
    public void IsMissing_Should_HighlightOnlyUnavailableValues_When_SelectedIdentifiersAreAbsentFromCatalog()
    {
        var component = Render<TestSelect>(parameters => parameters
            .Add(select => select.Values, [OptionId, 3]));

        var chips = component.FindComponents<MudChip<int>>();

        chips.Single(chip => chip.Instance.Value == OptionId).Instance.Color.ShouldBe(Color.Default);
        chips.Single(chip => chip.Instance.Value == 3).Instance.Color.ShouldBe(Color.Warning);
        component.Markup.ShouldContain("Недоступные варианты сохранены");
    }

    [Fact(DisplayName = "Render should open and close menu when keyboard activation and escape are used")]
    public async Task Render_Should_OpenAndCloseMenu_When_KeyboardActivationAndEscapeAreUsed()
    {
        var component = Render<TestSelect>();

        await component.Find(".mud-menu-activator").KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        component.Find(".mud-menu-activator").GetAttribute("aria-expanded").ShouldBe("true");
        await Popovers.Find(".mud-menu-list-wrapper").KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });
        component.Find(".mud-menu-activator").GetAttribute("aria-expanded").ShouldBe("false");
    }

    private sealed class FormattedSelect(TestOptionsSource source)
        : AsyncMultiSelectOption<int, SelectOption<int>>(source.LoadAsync, "Option", "No options.")
    {
        protected override string GetOptionName(SelectOption<int> option) => $"Name: {option.Name}";
    }

    private sealed class TestSelect(TestOptionsSource source)
        : AsyncMultiSelectOption<int, SelectOption<int>>(source.LoadAsync, "Вариант", "Варианты не найдены.");

    private sealed class TestOptionsSource
    {
        public int Requests { get; private set; }

        public bool Fail { get; set; }

        public bool Empty { get; set; }

        public TaskCompletionSource<Result<IReadOnlyList<SelectOption<int>>>>? Pending { get; set; }

        public CancellationToken Cancellation { get; private set; }

        public async Task<Result<IReadOnlyList<SelectOption<int>>>> LoadAsync(
            CancellationToken cancellationToken)
        {
            Requests++;
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
                : items;
            return Result.Success(options);
        }
    }
}
