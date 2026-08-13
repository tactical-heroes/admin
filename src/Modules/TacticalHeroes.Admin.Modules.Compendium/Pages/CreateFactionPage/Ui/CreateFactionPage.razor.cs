using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Model;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui;

public partial class CreateFactionPage
{
    private readonly CreateFactionFormModel Faction = new();
    private readonly FormErrorState<CreateFactionFormModel> _errors = new();
    private readonly CreateFactionFormModelValidator _validator = new();

    [Inject]
    private CreateFactionApi CreateFactionApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    protected override async Task SaveAsync()
    {
        _errors.Clear();

        Result<Guid> result = await CreateFactionApi.CreateAsync(
            Faction,
            LifetimeToken);

        if (result.IsFailure)
        {
            _errors.Handle(result.Errors, Snackbar);
            return;
        }

        Snackbar.Add("Фракция создана", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Faction(result.Value));
    }
}
