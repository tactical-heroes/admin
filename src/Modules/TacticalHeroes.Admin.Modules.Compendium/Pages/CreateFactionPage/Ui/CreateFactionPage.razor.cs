using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.CreateFactionPage.Ui;

public partial class CreateFactionPage
{
    private readonly CreateFactionFormModel Faction = new();
    private MudForm? _form;
    private bool _isValid;
    private bool _saving;
    private IReadOnlyDictionary<string, string[]> _fieldErrors =
        new Dictionary<string, string[]>();

    [Inject]
    private FactionsApi FactionsApi { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private async Task SaveAsync()
    {
        _saving = true;
        _fieldErrors = new Dictionary<string, string[]>();

        Result<Guid> result = await FactionsApi.CreateAsync(
            Faction,
            CancellationToken.None);

        if (result.IsFailure)
        {
            HandleErrors(result.Errors);
            _saving = false;
            return;
        }

        Snackbar.Add("Фракция создана", Severity.Success);
        Navigation.NavigateTo(CompendiumRoutes.Faction(result.Value));
    }

    private void HandleErrors(IReadOnlyList<Error> errors)
    {
        _fieldErrors = ApiErrorMessage.GetFieldErrors<CreateFactionFormModel>(errors);
        IReadOnlyList<Error> unhandledErrors =
            ApiErrorMessage.GetUnhandledErrors<CreateFactionFormModel>(errors);

        if (unhandledErrors.Count > 0)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(unhandledErrors), Severity.Error);
        }
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
