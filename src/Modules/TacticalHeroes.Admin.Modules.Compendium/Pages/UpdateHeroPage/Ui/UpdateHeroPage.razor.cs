using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Model;
using TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Api;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Ui;

public partial class UpdateHeroPage(
    UpdateHeroApi updateHeroApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudUpdateFormComponentBase<HeroFormModel, HeroFormModelValidator>(
        updateHeroApi.GetAsync,
        updateHeroApi.UpdateAsync,
        "Герой сохранён",
        CompendiumRoutes.Heroes,
        snackbar,
        navigation)
{
}
