using System.Net;

namespace TacticalHeroes.Admin.Api.Authentication;

internal sealed class SessionRefreshHandler(SessionRefreshState refreshState) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        var refreshRequested = response.IsSuccessStatusCode
            && response.Headers.TryGetValues(SessionRefreshState.HeaderName, out var values)
            && values.Any(value => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));
        if (response.StatusCode == HttpStatusCode.Unauthorized || refreshRequested)
        {
            refreshState.RequireRefresh();
        }

        return response;
    }
}
