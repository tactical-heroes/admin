using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Ui;

public partial class UpdateFactionPage
{
    private MudForm? _form;
    private bool _isValid;
    private bool _loading;
    private bool _saving;
    private IReadOnlyDictionary<string, string[]> _fieldErrors =
        new Dictionary<string, string[]>();

    [Inject]
    private FactionsApi FactionsApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    [PersistentState(AllowUpdates = true)]
    public UpdateFactionFormModel? Faction { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public Guid? LoadedId { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (LoadedId != Id)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        Faction = null;
        LoadError = null;
        LoadedId = Id;
        _fieldErrors = new Dictionary<string, string[]>();

        Result<UpdateFactionFormModel> result = await FactionsApi.GetAsync(
            Id,
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

        Result result = await FactionsApi.UpdateAsync(
            Id,
            Faction,
            CancellationToken.None);

        if (result.IsFailure)
        {
            HandleErrors(result.Errors);
            _saving = false;
            return;
        }

        Snackbar.Add("Фракция сохранена", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Factions);
    }

    private void HandleErrors(IReadOnlyList<Error> errors)
    {
        _fieldErrors = ApiErrorMessage.GetFieldErrors<UpdateFactionFormModel>(errors);
        IReadOnlyList<Error> unhandledErrors =
            ApiErrorMessage.GetUnhandledErrors<UpdateFactionFormModel>(errors);

        if (unhandledErrors.Count > 0)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(unhandledErrors), Severity.Error);
        }
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
        {
            return;
        }

        await _form.ValidateAsync();

        if (_isValid)
        {
            await SaveAsync();
        }
    }
}
