using Microsoft.AspNetCore.Components;
using MudBlazor;
using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Users.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Widgets.Users.UserList;

public partial class UserListWidget
{
    private const int PageSize = 10;
    private bool _loading;
    private string? _emailFilter;

    [Inject]
    private UsersApi UsersApi { get; set; } = null!;

    [Parameter]
    public int PageNumber { get; set; } = 1;

    [Parameter]
    public string? Email { get; set; }

    [Parameter]
    public EventCallback<string?> EmailChanged { get; set; }

    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    [PersistentState(AllowUpdates = true)]
    public PageResult<UserSummary>? Page { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageNumber { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadedEmail { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        string? normalizedEmail = NormalizeEmail(Email);
        _emailFilter = normalizedEmail;

        if (LoadedPageNumber != PageNumber ||
            !string.Equals(LoadedEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            await LoadPageAsync(PageNumber, normalizedEmail);
        }
    }

    private async Task SearchAsync()
    {
        await EmailChanged.InvokeAsync(NormalizeEmail(_emailFilter));
    }

    private async Task ChangePageAsync(int pageNumber)
    {
        await PageNumberChanged.InvokeAsync(pageNumber);
    }

    private async Task RetryAsync()
    {
        await LoadPageAsync(PageNumber, NormalizeEmail(Email));
    }

    private async Task LoadPageAsync(int pageNumber, string? email)
    {
        _loading = true;
        LoadError = null;
        LoadedPageNumber = pageNumber;
        LoadedEmail = email;

        try
        {
            Page = await UsersApi.GetPageAsync(pageNumber, PageSize, email);
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
