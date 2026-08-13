using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Ui;

public partial class RoleListPage(RoleListApi roleListApi)
{
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
    public PagedListState<RoleListItem>? PersistedListState { get; set; }

    private PagedListState<RoleListItem> ListState => PersistedListState ??= new();

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
        Navigation.NavigateTo(IdentityRoutes.RolesPage(pageNumber, CurrentPageSize));
    }

    private void ChangePageSize(int pageSize)
    {
        Navigation.NavigateTo(IdentityRoutes.RolesPage(pageSize: pageSize));
    }

    private async Task ConfirmDeleteAsync(RoleListItem role)
    {
        if (!await DialogService.ConfirmDeleteAsync("роль", role.Name))
        {
            return;
        }

        Result result = await ListState.DeleteAsync(
            role.Id,
            cancellationToken => roleListApi.DeleteAsync(role.Id, cancellationToken),
            LifetimeToken);

        if (result.IsFailure)
        {
            Snackbar.Add(ApiErrorMessage.FromErrors(result.Errors), Severity.Error);
            return;
        }

        Snackbar.Add("Роль удалена", Severity.Success);

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
            cancellationToken => roleListApi.GetPageAsync(
                CurrentPageNumber,
                CurrentPageSize,
                cancellationToken),
            cancellationToken: LifetimeToken);
    }
}
