using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Ui;

public partial class CreateUnitPage(
    CreateUnitApi createUnitApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudCreateFormComponentBase<CreateUnitFormModel, CreateUnitFormModelValidator>(
        createUnitApi.CreateAsync,
        "Юнит создан",
        CompendiumRoutes.Unit,
        snackbar,
        navigation)
{
}
