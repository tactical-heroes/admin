using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using TacticalHeroes.Admin.Api.Authentication;
using TacticalHeroes.Admin.Modules.Identity;

namespace TacticalHeroes.Admin.Client.App.Routing;

public partial class SessionRefreshRedirect(SessionRefreshState refreshState, NavigationManager navigation) : IDisposable
{
    private bool _refreshStarted;

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
            && navigation.ToAbsoluteUri(context.TargetLocation)
                != navigation.ToAbsoluteUri(IdentityRoutes.AuthenticationRefresh))
        {
            context.PreventNavigation();
            RefreshSession();
        }
    }

    private void RefreshSession()
    {
        if (_refreshStarted)
        {
            return;
        }

        _refreshStarted = true;
        navigation.NavigateTo(IdentityRoutes.AuthenticationRefresh, forceLoad: true, replace: true);
    }
}
