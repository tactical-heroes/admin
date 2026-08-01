using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Modules.Compendium.Features.Factions.DeleteFaction;

public partial class DeleteFactionPanel
{
    private bool _confirming;

    [Parameter, EditorRequired]
    public string FactionName { get; set; } = string.Empty;

    [Parameter]
    public bool Deleting { get; set; }

    [Parameter]
    public EventCallback OnDelete { get; set; }

    private void RequestConfirmation()
    {
        _confirming = true;
    }

    private void CancelConfirmation()
    {
        _confirming = false;
    }

    private async Task ConfirmAsync()
    {
        await OnDelete.InvokeAsync();
    }
}
