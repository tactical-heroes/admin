using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Ui;

public partial class UpdateFactionPage
{
    private readonly FormErrorState<UpdateFactionFormModel> _errors = new();
    private readonly UpdateFactionFormModelValidator _validator = new();
    private bool _loading;

    [Inject]
    private UpdateFactionApi UpdateFactionApi { get; set; } = null!;

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
        _errors.Clear();

        Result<UpdateFactionFormModel> result = await UpdateFactionApi.GetAsync(
            Id,
            LifetimeToken);

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

    protected override async Task SaveAsync()
    {
        if (Faction is null)
        {
            return;
        }

        _errors.Clear();

        Result result = await UpdateFactionApi.UpdateAsync(
            Id,
            Faction,
            LifetimeToken);

        if (result.IsFailure)
        {
            _errors.Handle(result.Errors, Snackbar);
            return;
        }

        Snackbar.Add("Фракция сохранена", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Factions);
    }
}
