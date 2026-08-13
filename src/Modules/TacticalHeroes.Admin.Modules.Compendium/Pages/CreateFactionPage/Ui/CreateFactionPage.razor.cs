using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui;

public partial class CreateFactionPage(CreateFactionApi createFactionApi)
{
    protected override Task<Result<Guid>> SaveCoreAsync()
    {
        return createFactionApi.CreateAsync(Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Result<Guid> result)
    {
        Snackbar.Add("Фракция создана", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Faction(result.Value));
    }
}
