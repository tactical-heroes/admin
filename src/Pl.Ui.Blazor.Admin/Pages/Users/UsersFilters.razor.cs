using Common.Enums;
using Common.SearchParams;

using Microsoft.AspNetCore.Components;

namespace Pl.Ui.Blazor.Admin.Pages.Users;

public partial class UsersFilters
{
    [Parameter] public UsersSearchParams SearchParams { get; set; } = default!;

    [Parameter] public EventCallback OnChanged { get; set; }

    private Task OnSearchChanged(string value)
    {
        SearchParams.SearchQuery = value;
        return Task.CompletedTask;
    }

    private Task OnSearchDebounced(string _)
    {
        return OnChanged.InvokeAsync();
    }

    private async Task OnRoleChanged(Role? value)
    {
        SearchParams.Role = value;
        await OnChanged.InvokeAsync();
    }
}
