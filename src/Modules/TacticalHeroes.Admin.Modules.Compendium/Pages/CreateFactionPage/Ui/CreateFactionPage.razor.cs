using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui;

public partial class CreateFactionPage(
    CreateFactionApi createFactionApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudFormComponentBase<CreateFactionFormModel, CreateFactionFormModelValidator>(
        snackbar,
        navigation)
{
    protected override Task<Result<Guid>> SaveCoreAsync()
    {
        return createFactionApi.CreateAsync(Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Guid id)
    {
        Snackbar.Add("Фракция создана", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Faction(id));
    }
}
