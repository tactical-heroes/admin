using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Compendium.Widgets.Factions.FactionList;

public partial class FactionListWidget
{
    private bool _loading;
    private Guid? _deletingId;

    [Inject]
    private FactionsApi FactionsApi { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public int PageNumber { get; set; } = 1;

    [Parameter]
    public int PageSize { get; set; } = PaginationOptions.DefaultPageSize;

    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    [PersistentState(AllowUpdates = true)]
    public PaginationResult<FactionListItem>? Page { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageNumber { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageSize { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (LoadedPageNumber != PageNumber || LoadedPageSize != PageSize)
        {
            await LoadPageAsync(PageNumber, PageSize);
        }
    }

    private Task ChangePageAsync(int pageNumber)
    {
        return PageNumberChanged.InvokeAsync(pageNumber);
    }

    private Task ChangePageSizeAsync(int pageSize)
    {
        return PageSizeChanged.InvokeAsync(pageSize);
    }

    private Task RetryAsync()
    {
        return LoadPageAsync(PageNumber, PageSize);
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

        Result result = await FactionsApi.DeleteAsync(id);

        if (result.IsFailure)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(result.Errors), Severity.Error);
            _deletingId = null;
            return;
        }

        Snackbar.Add("Фракция удалена", Severity.Success);

        if (Page?.Items.Count == 1 && PageNumber > 1)
        {
            await PageNumberChanged.InvokeAsync(PageNumber - 1);
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
            await FactionsApi.GetPageAsync(pageNumber, pageSize);

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
