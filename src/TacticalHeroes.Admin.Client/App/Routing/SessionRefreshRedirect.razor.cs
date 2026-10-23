using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using TacticalHeroes.Admin.Api.Authentication;
using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Client.App.Routing;

public sealed partial class SessionRefreshRedirect(SessionRefreshState refreshState, NavigationManager navigation)
    : IDisposable
{
    private string? _refreshUri;

    protected override void OnInitialized()
    {
        refreshState.RefreshRequested += HandleRefreshRequested;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (refreshState.IsRequired)
        {
            RefreshSession();
        }
    }

    public void Dispose()
    {
        refreshState.RefreshRequested -= HandleRefreshRequested;
        GC.SuppressFinalize(this);
    }

    private void HandleRefreshRequested()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void HandleLocationChanging(LocationChangingContext context)
    {
        if (refreshState.IsRequired
            && (_refreshUri is null
                || navigation.ToAbsoluteUri(context.TargetLocation) != navigation.ToAbsoluteUri(_refreshUri)))
        {
            context.PreventNavigation();
            RefreshSession();
        }
    }

    private void RefreshSession()
    {
        if (_refreshUri is not null)
        {
            return;
        }

        var currentUri = new Uri(navigation.Uri);
        _refreshUri = IdentityRoutes.Refresh($"{currentUri.PathAndQuery}{currentUri.Fragment}");
        navigation.NavigateTo(_refreshUri, forceLoad: true, replace: true);
    }
}
