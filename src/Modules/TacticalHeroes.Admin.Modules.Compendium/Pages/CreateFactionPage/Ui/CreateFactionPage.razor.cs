using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui;

public partial class CreateFactionPage(
    CreateFactionApi createFactionApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudCreateFormComponentBase<CreateFactionFormModel, CreateFactionFormModelValidator>(
        createFactionApi.CreateAsync,
        "Фракция создана",
        CompendiumRoutes.Faction,
        snackbar,
        navigation)
{
}
