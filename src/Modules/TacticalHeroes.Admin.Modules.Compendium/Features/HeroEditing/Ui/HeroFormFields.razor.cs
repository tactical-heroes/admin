using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Ui;

public partial class HeroFormFields(FactionOptionsApi factionOptionsApi) : CancelableComponentBase
{
    [Parameter, EditorRequired]
    public HeroFormModel Model { get; set; } = new();

    [Parameter, EditorRequired]
    public FormErrorState<HeroFormModel> Errors { get; set; } = new();

    [PersistentState(AllowUpdates = true)]
    public List<FactionOption>? Factions { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? FactionLoadError { get; set; }

    private bool IsLoadingFactions { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Factions is null)
        {
            await LoadFactionsAsync();
        }
    }

    private async Task LoadFactionsAsync()
    {
        IsLoadingFactions = true;
        FactionLoadError = null;

        try
        {
            var result = await factionOptionsApi.GetAllAsync(LifetimeToken);
            if (result.IsFailure)
            {
                FactionLoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Factions = [.. result.Value];
        }
        finally
        {
            IsLoadingFactions = false;
        }
    }
}
