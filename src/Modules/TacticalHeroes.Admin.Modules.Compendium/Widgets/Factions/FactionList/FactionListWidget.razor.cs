using Microsoft.AspNetCore.Components;
using TacticalHeroes.Admin.Api.Errors;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Api;
using TacticalHeroes.Admin.Modules.Compendium.Entities.Factions.Model;
using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Modules.Compendium.Widgets.Factions.FactionList;

public partial class FactionListWidget
{
    private const int PageSize = 10;
    private bool _loading;

    [Inject]
    private FactionsApi FactionsApi { get; set; } = null!;

    [Parameter]
    public int PageNumber { get; set; } = 1;

    [Parameter]
    public EventCallback<int> PageNumberChanged { get; set; }

    [PersistentState(AllowUpdates = true)]
    public PageResult<FactionSummary>? Page { get; set; }

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
            Page = await FactionsApi.GetPageAsync(pageNumber, PageSize);
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
