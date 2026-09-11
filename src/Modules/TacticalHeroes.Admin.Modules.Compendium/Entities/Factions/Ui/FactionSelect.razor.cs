using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Ui.Common;

namespace TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Ui;

public partial class FactionSelect(FactionOptionsApi factionOptionsApi) : CancelableComponentBase
{
    private const int FactionOptionLimit = 20;
    private const int FactionSearchMinLength = 3;

    [Parameter]
    public Guid Value { get; set; }

    [Parameter]
    public EventCallback<Guid> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<Guid>>? For { get; set; }

    [Parameter]
    public bool Error { get; set; }

    [Parameter]
    public string? ErrorText { get; set; }

    [PersistentState(AllowUpdates = true)]
    public FactionSelectOption? SelectedFaction { get; set; }

    private IReadOnlyList<FactionSelectOption> Factions { get; set; } = [];

    private Guid? LoadedFactionId { get; set; }

    private string? FactionLoadError { get; set; }

    private string? FactionSearchError { get; set; }

    private bool IsLoadingFaction { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Value != Guid.Empty && SelectedFaction?.Id != Value && LoadedFactionId != Value)
        {
            await LoadSelectedFactionAsync();
        }
    }

    private async Task LoadSelectedFactionAsync()
    {
        var factionId = Value;
        LoadedFactionId = factionId;
        IsLoadingFaction = true;
        FactionLoadError = null;

        try
        {
            var result = await factionOptionsApi.SearchAsync(null, FactionOptionLimit, LifetimeToken);
            if (Value != factionId)
            {
                return;
            }

            if (result.IsFailure)
            {
                FactionLoadError = ApiErrorMessage.FromErrors(result.Errors);
                return;
            }

            Factions = result.Value;
            SelectedFaction = Factions.FirstOrDefault(faction => faction.Id == factionId);
        }
        finally
        {
            IsLoadingFaction = false;
        }
    }

    private async Task<IEnumerable<Guid>> SearchFactionsAsync(string? search, CancellationToken cancellationToken)
    {
        search = search?.Trim();
        if (search is null || search.Length < FactionSearchMinLength)
        {
            Factions = [];
            FactionSearchError = null;
            await InvokeAsync(StateHasChanged);
            return [];
        }

        using var source = CancellationTokenSource.CreateLinkedTokenSource(LifetimeToken, cancellationToken);
        var result = await factionOptionsApi.SearchAsync(search, FactionOptionLimit, source.Token);
        source.Token.ThrowIfCancellationRequested();
        FactionSearchError = result.IsFailure ? ApiErrorMessage.FromErrors(result.Errors) : null;
        Factions = result.IsSuccess ? result.Value : [];
        await InvokeAsync(StateHasChanged);
        return Factions.Select(faction => faction.Id);
    }

    private async Task SetFactionAsync(Guid id)
    {
        Value = id;
        SelectedFaction = Factions.FirstOrDefault(faction => faction.Id == id)
            ?? (SelectedFaction?.Id == id ? SelectedFaction : null);
        await ValueChanged.InvokeAsync(id);
    }

    private string GetFactionName(Guid id)
    {
        return SelectedFaction?.Id == id
            ? SelectedFaction.Name
            : Factions.FirstOrDefault(faction => faction.Id == id)?.Name ?? string.Empty;
    }
}
