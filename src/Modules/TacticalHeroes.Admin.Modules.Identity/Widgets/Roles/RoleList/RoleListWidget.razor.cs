using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Widgets.Roles.RoleList;

public partial class RoleListWidget
{
    private bool _loading;
    private Guid? _deletingId;

    [Inject]
    private RolesApi RolesApi { get; set; } = null!;

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
    public PageResult<RoleSummary>? Page { get; set; }

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

    private async Task ConfirmDeleteAsync(RoleSummary role)
    {
        var parameters = new DialogParameters
        {
            [nameof(DeleteConfirmationDialog.EntityType)] = "роль",
            [nameof(DeleteConfirmationDialog.EntityName)] = role.Name,
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

        await DeleteAsync(role.Id);
    }

    private async Task DeleteAsync(Guid id)
    {
        _deletingId = id;

        try
        {
            await RolesApi.DeleteAsync(id);
            Snackbar.Add("Роль удалена", Severity.Success);

            if (Page?.Items.Count == 1 && PageNumber > 1)
            {
                await PageNumberChanged.InvokeAsync(PageNumber - 1);
            }
            else
            {
                await RetryAsync();
            }
        }
        catch (Exception exception)
        {
            Snackbar.Add(ApiErrorMessage.FromException(exception), Severity.Error);
        }
        finally
        {
            _deletingId = null;
        }
    }

    private async Task LoadPageAsync(int pageNumber, int pageSize)
    {
        _loading = true;
        LoadError = null;
        LoadedPageNumber = pageNumber;
        LoadedPageSize = pageSize;

        try
        {
            Page = await RolesApi.GetPageAsync(pageNumber, pageSize);
        }
        catch (Exception exception)
        {
            Page = null;
            LoadError = ApiErrorMessage.FromException(exception);
        }
        finally
        {
            _loading = false;
        }
    }
}
