using Microsoft.AspNetCore.Components;

using MudBlazor;

using PANiXiDA.Core.ResultPattern;

using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Api;
using TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Model;
using TacticalHeroes.Admin.Shared.Errors;
using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Pages.UserListPage.Ui;

public partial class UserListPage(
    UserListApi userListApi,
    IDialogService dialogService,
    ISnackbar snackbar,
    NavigationManager navigation)
{
    private string? _emailFilter;

    [SupplyParameterFromQuery(Name = "page")]
    public int? PageNumber { get; set; }

    [SupplyParameterFromQuery(Name = "email")]
    public string? Email { get; set; }

    [SupplyParameterFromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    [PersistentState(AllowUpdates = true)]
    public PagedListState<UserListItem>? PersistedListState { get; set; }

    private PagedListState<UserListItem> ListState => PersistedListState ??= new();

    private int CurrentPageNumber => PageNumber is > 0
        ? PageNumber.Value
        : 1;

    private int CurrentPageSize => PaginationOptions.NormalizePageSize(PageSize);

    protected override async Task OnParametersSetAsync()
    {
        string? normalizedEmail = NormalizeEmail(Email);
        _emailFilter = normalizedEmail;

        if (!ListState.Matches(CurrentPageNumber, CurrentPageSize, normalizedEmail))
        {
            await LoadPageAsync();
        }
    }

    private void ApplyEmailFilter()
    {
        navigation.NavigateTo(IdentityRoutes.UsersPage(
            NormalizeEmail(_emailFilter),
            pageSize: CurrentPageSize));
    }

    private void ApplyDynamicEmailFilter(string value)
    {
        string? normalizedEmail = NormalizeEmail(value);

        if (normalizedEmail is not null && normalizedEmail.Length < 3)
        {
            return;
        }

        if (!string.Equals(
                normalizedEmail,
                NormalizeEmail(Email),
                StringComparison.OrdinalIgnoreCase))
        {
            navigation.NavigateTo(IdentityRoutes.UsersPage(
                normalizedEmail,
                pageSize: CurrentPageSize));
        }
    }

    private void ResetFilters()
    {
        _emailFilter = null;
        navigation.NavigateTo(IdentityRoutes.UsersPage(pageSize: CurrentPageSize));
    }

    private void ChangePage(int pageNumber)
    {
        navigation.NavigateTo(IdentityRoutes.UsersPage(
            NormalizeEmail(Email),
            pageNumber,
            CurrentPageSize));
    }

    private void ChangePageSize(int pageSize)
    {
        navigation.NavigateTo(IdentityRoutes.UsersPage(
            NormalizeEmail(Email),
            pageSize: pageSize));
    }

    private async Task ConfirmDeleteAsync(UserListItem user)
    {
        if (!await dialogService.ConfirmDeleteAsync("пользователя", user.UserName))
        {
            return;
        }

        Result result = await ListState.DeleteAsync(
            user.Id,
            cancellationToken => userListApi.DeleteAsync(user.Id, cancellationToken),
            LifetimeToken);

        if (result.IsFailure)
        {
            snackbar.Add(ApiErrorMessage.FromErrors(result.Errors), Severity.Error);
            return;
        }

        snackbar.Add("Пользователь удалён", Severity.Success);

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
        string? normalizedEmail = NormalizeEmail(Email);

        return ListState.LoadAsync(
            CurrentPageNumber,
            CurrentPageSize,
            cancellationToken => userListApi.GetPageAsync(
                CurrentPageNumber,
                CurrentPageSize,
                normalizedEmail,
                cancellationToken),
            LifetimeToken,
            normalizedEmail);
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim();
    }

    private static Color GetStatusColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "active" => Color.Success,
            "blocked" => Color.Error,
            _ => Color.Default,
        };
    }
}
