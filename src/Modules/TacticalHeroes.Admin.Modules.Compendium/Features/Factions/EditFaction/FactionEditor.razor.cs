using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Features.Factions.EditFaction;

public partial class FactionEditor
{
    private bool _loading;
    private bool _saving;
    private IReadOnlyDictionary<string, string[]> _fieldErrors =
        new Dictionary<string, string[]>();

    [Inject]
    private FactionsApi FactionsApi { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public Guid? Id { get; set; }

    [Parameter]
    public EventCallback Completed { get; set; }

    [PersistentState]
    public FactionDetails? Faction { get; set; }

    [PersistentState]
    public string? LoadError { get; set; }

    private bool IsNew => !Id.HasValue;

    protected override async Task OnParametersSetAsync()
    {
        if (!Id.HasValue)
        {
            if (Faction is null || Faction.Id.HasValue)
            {
                Faction = new FactionDetails();
            }

            LoadError = null;
            _fieldErrors = new Dictionary<string, string[]>();
            return;
        }

        if (Faction?.Id != Id.Value)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        if (!Id.HasValue)
        {
            return;
        }

        _loading = true;
        LoadError = null;
        _fieldErrors = new Dictionary<string, string[]>();

        Result<FactionDetails> result = await FactionsApi.GetAsync(
            Id.Value,
            CancellationToken.None);

        if (result.IsFailure)
        {
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Faction = result.Value;
        }

        _loading = false;
    }

    private async Task SaveAsync()
    {
        if (Faction is null)
        {
            return;
        }

        _saving = true;
        _fieldErrors = new Dictionary<string, string[]>();

        if (Faction.Id.HasValue)
        {
            Result result = await FactionsApi.UpdateAsync(Faction, CancellationToken.None);

            if (result.IsFailure)
            {
                HandleErrors(result.Errors);
                _saving = false;
                return;
            }

            Snackbar.Add("Фракция сохранена", Severity.Success);
        }
        else
        {
            Result<Guid> result = await FactionsApi.CreateAsync(Faction, CancellationToken.None);

            if (result.IsFailure)
            {
                HandleErrors(result.Errors);
                _saving = false;
                return;
            }

            Faction.Id = result.Value;
            Snackbar.Add("Фракция создана", Severity.Success);
        }

        _saving = false;
        await Completed.InvokeAsync();
    }

    private void HandleErrors(IReadOnlyList<Error> errors)
    {
        _fieldErrors = ApiErrorMessage.GetFieldErrors(errors, MapField);
        IReadOnlyList<Error> unhandledErrors =
            ApiErrorMessage.GetUnhandledErrors(errors, MapField);

        if (unhandledErrors.Count > 0)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(unhandledErrors), Severity.Error);
        }
    }

    private static string? MapField(string field)
    {
        if (string.Equals(field, nameof(FactionDetails.Name), StringComparison.OrdinalIgnoreCase)
            || string.Equals(field, "FactionName", StringComparison.OrdinalIgnoreCase))
        {
            return nameof(FactionDetails.Name);
        }

        if (string.Equals(
                field,
                nameof(FactionDetails.Description),
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(field, "FactionDescription", StringComparison.OrdinalIgnoreCase))
        {
            return nameof(FactionDetails.Description);
        }

        return null;
    }
}
