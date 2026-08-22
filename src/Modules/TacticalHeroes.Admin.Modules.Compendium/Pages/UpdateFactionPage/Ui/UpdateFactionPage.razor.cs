using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Ui;

public partial class UpdateFactionPage(
    UpdateFactionApi updateFactionApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<UpdateFactionFormModel, UpdateFactionFormModelValidator>(
        snackbar,
        navigation)
{
    protected override Task<Result<UpdateFactionFormModel>> LoadCoreAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return updateFactionApi.GetAsync(id, cancellationToken);
    }

    protected override Task<Result<Guid>> SaveCoreAsync()
    {
        return updateFactionApi.UpdateAsync(Id, Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Guid id)
    {
        Snackbar.Add("Фракция сохранена", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Factions);
    }
}
