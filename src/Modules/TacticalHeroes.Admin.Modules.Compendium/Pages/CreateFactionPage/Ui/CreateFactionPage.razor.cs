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
    private MudForm? _form;
    private bool _isValid;
    private bool _saving;

    [Inject]
    private FactionsApi FactionsApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private async Task SaveAsync()
    {
        _saving = true;
        _errors.Clear();

        Result<Guid> result = await FactionsApi.CreateAsync(
            Faction,
            LifetimeToken);

        if (result.IsFailure)
        {
            _errors.Handle(result.Errors, Snackbar);
            _saving = false;
            return;
        }

        Snackbar.Add("Фракция создана", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Faction(result.Value));
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
        {
            return;
        }

        await _form.ValidateAsync();

        if (_isValid)
        {
            await SaveAsync();
        }
    }
}
