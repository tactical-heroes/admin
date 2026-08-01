using MudBlazor.Services;
using TacticalHeroes.Admin.Modules.Compendium.Features.Factions.DeleteFaction;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Features.Factions.DeleteFaction;

public sealed class DeleteFactionPanelTests : BunitContext
{
    public DeleteFactionPanelTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Requires confirmation before invoking delete")]
    public void Delete_Should_InvokeCallback_When_DeletionIsConfirmed()
    {
        var deleted = false;
        var component = Render<DeleteFactionPanel>(parameters => parameters
            .Add(panel => panel.FactionName, "Northern Alliance")
            .Add(panel => panel.OnDelete, () => deleted = true));

        component.Find(".delete-request").Click();

        deleted.ShouldBeFalse();
        component.Find(".delete-confirm").Click();
        component.WaitForAssertion(() => deleted.ShouldBeTrue());
    }
}
