using Microsoft.AspNetCore.Components;
using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Api;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Identity.Widgets.Roles.RoleList;

public partial class RoleListWidget
{
    private const int PageSize = 10;
    private bool _loading;

    [Inject]
    private RolesApi RolesApi { get; set; } = null!;

    [Parameter]
    public int PageNumber { get; set; } = 1;

    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    [PersistentState(AllowUpdates = true)]
    public PageResult<RoleSummary>? Page { get; set; }

    [PersistentState(AllowUpdates = true)]
    public string? LoadError { get; set; }

    [PersistentState(AllowUpdates = true)]
    public int? LoadedPageNumber { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (LoadedPageNumber != PageNumber)
        {
            await LoadPageAsync(PageNumber);
        }
    }

    private async Task ChangePageAsync(int pageNumber)
    {
        await PageNumberChanged.InvokeAsync(pageNumber);
    }

    private async Task RetryAsync()
    {
        await LoadPageAsync(PageNumber);
    }

    private async Task LoadPageAsync(int pageNumber)
    {
        _loading = true;
        LoadError = null;
        LoadedPageNumber = pageNumber;

        try
        {
            Page = await RolesApi.GetPageAsync(pageNumber, PageSize);
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
