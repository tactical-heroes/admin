using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Ui;

public partial class HeroFormFields(FactionOptionsApi factionOptionsApi) : CancelableComponentBase
{
    private const int FactionOptionLimit = 20;

    [Parameter, EditorRequired]
    public HeroFormModel Model { get; set; } = new();

    [Parameter, EditorRequired]
    public FormErrorState<HeroFormModel> Errors { get; set; } = new();

    [PersistentState(AllowUpdates = true)]
    public FactionOption? SelectedFaction { get; set; }

    private IReadOnlyList<FactionOption> Factions { get; set; } = [];

    private string? FactionLoadError { get; set; }

    private string? FactionSearchError { get; set; }

    private bool IsLoadingFaction { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Model.FactionId != Guid.Empty && SelectedFaction?.Id != Model.FactionId)
        {
            await LoadSelectedFactionAsync();
        }
    }

    private async Task LoadSelectedFactionAsync()
    {
        var factionId = Model.FactionId;
        IsLoadingFaction = true;
        FactionLoadError = null;

        try
        {
            var result = await factionOptionsApi.GetAsync(factionId, LifetimeToken);
            if (Model.FactionId != factionId)
            {
                return;
            }

            if (result.IsFailure)
            {
                FactionLoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            SelectedFaction = result.Value;
        }
        finally
        {
            IsLoadingFaction = false;
        }
    }

    private async Task<IEnumerable<Guid>> SearchFactionsAsync(string? search, CancellationToken cancellationToken)
    {
        using var source = CancellationTokenSource.CreateLinkedTokenSource(LifetimeToken, cancellationToken);
        var result = await factionOptionsApi.SearchAsync(search, FactionOptionLimit, source.Token);
        source.Token.ThrowIfCancellationRequested();
        FactionSearchError = result.IsFailure ? ApiErrorMessage.FromErrors(result.Errors) : null;
        Factions = result.IsSuccess ? result.Value : [];
        await InvokeAsync(StateHasChanged);
        return Factions.Select(faction => faction.Id);
    }

    private void SetFaction(Guid id)
    {
        Model.FactionId = id;
        SelectedFaction = Factions.FirstOrDefault(faction => faction.Id == id)
            ?? (SelectedFaction?.Id == id ? SelectedFaction : null);
    }

    private string GetFactionName(Guid id)
    {
        return SelectedFaction?.Id == id
            ? SelectedFaction.Name
            : Factions.FirstOrDefault(faction => faction.Id == id)?.Name ?? string.Empty;
    }
}
