using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Ui;

public partial class FactionListPage
{
    [Inject]
    private FactionsApi FactionsApi { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    [SupplyParameterFromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [PersistentState(AllowUpdates = true)]
    public PagedListState<FactionListItem>? PersistedListState { get; set; }

    private PagedListState<FactionListItem> ListState => PersistedListState ??= new();

    private int CurrentPageNumber => PageNumber is > 0
        ? PageNumber.Value
        : 1;

    private int CurrentPageSize => PaginationOptions.NormalizePageSize(PageSize);

    protected override async Task OnParametersSetAsync()
    {
        if (!ListState.Matches(CurrentPageNumber, CurrentPageSize))
        {
            await LoadPageAsync();
        }
    }

    private void ChangePage(int pageNumber)
    {
        Navigation.NavigateTo(CompendiumRoutes.FactionsPage(pageNumber, CurrentPageSize));
    }

    private void ChangePageSize(int pageSize)
    {
        Navigation.NavigateTo(CompendiumRoutes.FactionsPage(pageSize: pageSize));
    }

    private async Task ConfirmDeleteAsync(FactionListItem faction)
    {
        if (!await DialogService.ConfirmDeleteAsync("фракцию", faction.Name))
        {
            return;
        }

        await DeleteAsync(faction.Id);
    }

    private async Task DeleteAsync(Guid id)
    {
        Result result = await ListState.DeleteAsync(
            id,
            cancellationToken => FactionsApi.DeleteAsync(id, cancellationToken),
            CancellationToken.None);

        if (result.IsFailure)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(result.Errors), Severity.Error);
            return;
        }

        Snackbar.Add("Фракция удалена", Severity.Success);

        if (ListState.Page?.Items.Count == 1 && CurrentPageNumber > 1)
        {
            ChangePage(CurrentPageNumber - 1);
        }
        else
        {
            await LoadPageAsync();
        }
    }

    private Task LoadPageAsync()
    {
        return ListState.LoadAsync(
            CurrentPageNumber,
            CurrentPageSize,
            cancellationToken => FactionsApi.GetPageAsync(
                CurrentPageNumber,
                CurrentPageSize,
                cancellationToken),
            CancellationToken.None);
    }
}
