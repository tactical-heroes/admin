using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Ui;

public partial class UpdateUnitPage(
    UpdateUnitApi updateUnitApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<UpdateUnitFormModel, UpdateUnitFormModelValidator>(
        updateUnitApi.GetAsync,
        updateUnitApi.UpdateAsync,
        "Юнит сохранён",
        CompendiumRoutes.Units,
        snackbar,
        navigation)
{
}
