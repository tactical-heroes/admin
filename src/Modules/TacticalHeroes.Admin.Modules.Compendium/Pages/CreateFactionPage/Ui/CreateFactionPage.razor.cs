using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui;

public partial class CreateFactionPage
{
    [Inject]
    private CreateFactionApi CreateFactionApi { get; set; } = null!;

    protected override Task<Result<Guid>> SaveCoreAsync()
    {
        return CreateFactionApi.CreateAsync(Model, LifetimeToken);
    }

    protected override void OnSaveSucceeded(Result<Guid> result)
    {
        Snackbar.Add("Фракция создана", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Faction(result.Value));
    }
}
