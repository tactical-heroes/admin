using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui;

public partial class CreateFactionPage
{
    [Inject]
    private CreateFactionApi CreateFactionApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    protected override Task ExecuteSaveAsync()
    {
        return SaveAsync(
            () => CreateFactionApi.CreateAsync(Model, LifetimeToken),
            id =>
            {
                Snackbar.Add("Фракция создана", Severity.Success);
                Navigation.NavigateTo(CompendiumRoutes.Faction(id));
            });
    }
}
