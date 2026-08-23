using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateFactionPage.Ui;

public partial class UpdateFactionPage(
    UpdateFactionApi updateFactionApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<UpdateFactionFormModel, UpdateFactionFormModelValidator>(
        updateFactionApi.GetAsync,
        updateFactionApi.UpdateAsync,
        "Фракция сохранена",
        CompendiumRoutes.Factions,
        snackbar,
        navigation)
{
}
