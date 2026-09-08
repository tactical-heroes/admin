using MudBlazor;
using MudBlazor.Services;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Inputs;

public sealed class EnumerationSelectTests : BunitContext
{
    public EnumerationSelectTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "GetDisplayName should map names to display names when items are provided")]
    public void GetDisplayName_Should_MapNamesToDisplayNames_When_ItemsAreProvided()
    {
        IEnumeration[] items =
        [
            new TestEnumeration("Active", "Активный"),
            new TestEnumeration("Blocked", "Заблокирован"),
        ];

        var component = Render<EnumerationSelect>(parameters => parameters
            .Add(select => select.Items, items)
            .Add(select => select.Value, "Blocked")
            .Add(select => select.ValueChanged, _ => { })
            .Add(select => select.Label, "Статус"));

        MudSelect<string> select = component.FindComponent<MudSelect<string>>().Instance;
        Func<string?, string?> toString = select.ToStringFunc
            ?? throw new InvalidOperationException("The display name formatter is not configured.");

        toString("Blocked").ShouldBe("Заблокирован");
        toString("Unknown").ShouldBe("Unknown");
        component.FindComponents<MudSelectItem<string>>()
            .Select(item => item.Instance.Value)
            .ShouldBe(["Active", "Blocked"]);
    }

    private sealed record TestEnumeration(
        string Name,
        string DisplayName) : IEnumeration;
}
