using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Ui;

public partial class UpdateFactionPage(UpdateFactionApi updateFactionApi)
{
    private bool _loading;

    [Parameter]
    public Guid Id { get; set; }

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
        LoadError = null;
        LoadedId = Id;
        Errors.Clear();

        Result<UpdateFactionFormModel> result = await updateFactionApi.GetAsync(
            Id,
            LifetimeToken);

        if (result.IsFailure)
        {
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Model = result.Value;
        }

        _loading = false;
    }

    protected override Task<Result> SaveCoreAsync()
    {
        return updateFactionApi.UpdateAsync(Id, Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Result result)
    {
        Snackbar.Add("Фракция сохранена", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Factions);
    }
}
