using MudBlazor.Services;

using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Modules.Compendium.Features.Factions.EditFaction;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Features.Factions.EditFaction;

public sealed class FactionEditFormTests : BunitContext
{
    public FactionEditFormTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Invokes save for a valid faction")]
    public void Submit_Should_InvokeSave_When_FactionIsValid()
    {
        var saved = false;
        var faction = new FactionDetails
        {
            Name = "Northern Alliance",
            Description = "A defensive coalition.",
        };
        var component = Render<FactionEditForm>(parameters => parameters
            .Add(form => form.Model, faction)
            .Add(form => form.OnSave, () => saved = true));

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() => saved.ShouldBeTrue());
    }
}
