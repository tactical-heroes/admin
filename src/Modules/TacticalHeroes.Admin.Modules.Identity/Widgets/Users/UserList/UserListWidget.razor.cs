using Microsoft.AspNetCore.Components;

using MudBlazor;

using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Shared.Model;
using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.Widgets.Users.UserList;

public partial class UserListWidget
{
    private bool _loading;
    private Guid? _deletingId;
    private string? _emailFilter;

    [Inject]
    private UsersApi UsersApi { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public int PageNumber { get; set; } = 1;

    [Parameter]
    public int PageSize { get; set; } = PaginationOptions.DefaultPageSize;

    [Parameter]
    public string? Email { get; set; }

    [Parameter]
    public EventCallback<string?> EmailChanged { get; set; }

    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    [PersistentState(AllowUpdates = true)]
    public PaginationResult<UserListItem>? Page { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageNumber { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageSize { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadedEmail { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        string? normalizedEmail = NormalizeEmail(Email);
        _emailFilter = normalizedEmail;

        if (LoadedPageNumber != PageNumber ||
            LoadedPageSize != PageSize ||
            !string.Equals(LoadedEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            await LoadPageAsync(PageNumber, PageSize, normalizedEmail);
        }
    }

    private Task ApplyEmailFilterAsync()
    {
        return EmailChanged.InvokeAsync(NormalizeEmail(_emailFilter));
    }

    private Task ApplyDynamicEmailFilterAsync(string value)
    {
        string? normalizedEmail = NormalizeEmail(value);

        if (normalizedEmail is not null && normalizedEmail.Length < 3)
        {
            return Task.CompletedTask;
        }

        if (string.Equals(normalizedEmail, NormalizeEmail(Email), StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        return EmailChanged.InvokeAsync(normalizedEmail);
    }

    private Task ResetFiltersAsync()
    {
        _emailFilter = null;
        return EmailChanged.InvokeAsync(null);
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
        return LoadPageAsync(PageNumber, PageSize, NormalizeEmail(Email));
    }

    private async Task ConfirmDeleteAsync(UserListItem user)
    {
        var parameters = new DialogParameters
        {
            [nameof(DeleteConfirmationDialog.EntityType)] = "пользователя",
            [nameof(DeleteConfirmationDialog.EntityName)] = user.UserName,
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

        await DeleteAsync(user.Id);
    }

    private async Task DeleteAsync(Guid id)
    {
        _deletingId = id;

        try
        {
            await UsersApi.DeleteAsync(id);
            Snackbar.Add("Пользователь удалён", Severity.Success);

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

    private async Task LoadPageAsync(int pageNumber, int pageSize, string? email)
    {
        _loading = true;
        LoadError = null;
        LoadedPageNumber = pageNumber;
        LoadedPageSize = pageSize;
        LoadedEmail = email;

        try
        {
            Page = await UsersApi.GetPageAsync(pageNumber, pageSize, email);
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
