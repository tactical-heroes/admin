using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Ui;

public partial class CreateHeroPage(
    CreateHeroApi createHeroApi,
    ISnackbar snackbar,
    NavigationManager navigation)
    : MudCreateFormComponentBase<CreateHeroFormModel, CreateHeroFormModelValidator>(
        createHeroApi.CreateAsync,
        "Герой создан",
        CompendiumRoutes.Hero,
        snackbar,
        navigation)
{
}
