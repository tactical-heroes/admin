using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Ui;

public partial class UpdateHeroPage(
    UpdateHeroApi updateHeroApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<UpdateHeroFormModel, UpdateHeroFormModelValidator>(
        updateHeroApi.GetAsync,
        updateHeroApi.UpdateAsync,
        "Герой сохранён",
        CompendiumRoutes.Heroes,
        snackbar,
        navigation)
{
}
