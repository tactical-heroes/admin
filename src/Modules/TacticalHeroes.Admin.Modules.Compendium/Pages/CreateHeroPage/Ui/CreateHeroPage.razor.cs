using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Features.HeroEditing.Model;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Api;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Ui;

public partial class CreateHeroPage(
    CreateHeroApi createHeroApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudCreateFormComponentBase<HeroFormModel, HeroFormModelValidator>(
        createHeroApi.CreateAsync,
        "Герой создан",
        CompendiumRoutes.Hero,
        snackbar,
        navigation)
{
}
