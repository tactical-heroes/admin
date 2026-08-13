using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Api;
using TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Model;
using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.Pages.FactionListPage.Ui;

public partial class FactionListPage
{
    private bool _loading;
    private Guid? _deletingId;

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
    public PaginationResult<FactionListItem>? Page { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageNumber { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageSize { get; set; }

    private int CurrentPageNumber => PageNumber is > 0
        ? PageNumber.Value
        : 1;

    private int CurrentPageSize => PaginationOptions.NormalizePageSize(PageSize);

    protected override async Task OnParametersSetAsync()
    {
        if (LoadedPageNumber != CurrentPageNumber || LoadedPageSize != CurrentPageSize)
        {
            await LoadPageAsync(CurrentPageNumber, CurrentPageSize);
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

    private Task RetryAsync()
    {
        return LoadPageAsync(CurrentPageNumber, CurrentPageSize);
    }

    private async Task ConfirmDeleteAsync(FactionListItem faction)
    {
        var parameters = new DialogParameters
        {
            [nameof(DeleteConfirmationDialog.EntityType)] = "фракцию",
            [nameof(DeleteConfirmationDialog.EntityName)] = faction.Name,
        };
        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.ExtraSmall,
        };
        IDialogReference dialog = await DialogService.ShowAsync<DeleteConfirmationDialog>(
            string.Empty,
            parameters,
            options);
        DialogResult? result = await dialog.Result;

        if (result is null || result.Canceled)
        {
            return;
        }

        await DeleteAsync(faction.Id);
    }

    private async Task DeleteAsync(Guid id)
    {
        _deletingId = id;

        Result result = await FactionsApi.DeleteAsync(id, CancellationToken.None);

        if (result.IsFailure)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(result.Errors), Severity.Error);
            _deletingId = null;
            return;
        }

        Snackbar.Add("Фракция удалена", Severity.Success);

        if (Page?.Items.Count == 1 && CurrentPageNumber > 1)
        {
            ChangePage(CurrentPageNumber - 1);
        }
        else
        {
            await RetryAsync();
        }

        _deletingId = null;
    }

    private async Task LoadPageAsync(int pageNumber, int pageSize)
    {
        _loading = true;
        LoadError = null;
        LoadedPageNumber = pageNumber;
        LoadedPageSize = pageSize;

        Result<PaginationResult<FactionListItem>> result =
            await FactionsApi.GetPageAsync(pageNumber, pageSize, CancellationToken.None);

        if (result.IsFailure)
        {
            Page = null;
            LoadError = ApiErrorMessage.FromErrors(result.Errors);
        }
        else
        {
            Page = result.Value;
        }

        _loading = false;
    }
}
