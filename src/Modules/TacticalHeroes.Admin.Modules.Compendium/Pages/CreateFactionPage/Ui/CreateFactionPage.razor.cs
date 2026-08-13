using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui;

public partial class CreateFactionPage
{
    [Inject]
    private CreateFactionApi CreateFactionApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    protected override async Task SaveAsync()
    {
        Errors.Clear();

        Result<Guid> result = await CreateFactionApi.CreateAsync(
            Model,
            LifetimeToken);

        if (result.IsFailure)
        {
            Errors.Handle(result.Errors, Snackbar);
            return;
        }

        Snackbar.Add("Фракция создана", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Faction(result.Value));
    }
}
