using System.Net;

using TacticalHeroes.Admin.Infrastructure.Authentication.Session;

using Yarp.ReverseProxy.Transforms;

namespace TacticalHeroes.Admin.Infrastructure.Proxy.Transforms;

internal sealed class PasswordChangeSessionRefreshTransform : ResponseTransform
{
    public override async ValueTask ApplyAsync(ResponseTransformContext context)
    {
        if (context.ProxyResponse?.StatusCode == HttpStatusCode.NoContent
            && HttpMethods.IsPost(context.HttpContext.Request.Method)
            && context.HttpContext.Request.Path == "/api/v1/auth/change-password")
        {
            await context.HttpContext.RequestServices
                .GetRequiredService<SessionCookieRefresher>()
                .RefreshAsync(context.HttpContext);
        }
    }
}
