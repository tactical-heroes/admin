using System.Net;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Infrastructure.Authentication.Login;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Modules.Identity;
using TacticalHeroes.Admin.Shared.Errors;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.Endpoints;

internal static class AuthenticationEndpointRouteBuilderExtensions
{
    internal static IEndpointRouteBuilder MapAdminAuthentication(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                IdentityRoutes.AuthenticationChallenge,
                (string? returnUrl) => TypedResults.Challenge(GetAuthenticationProperties(returnUrl)))
            .AllowAnonymous();

        endpoints.MapPost(
                IdentityRoutes.AuthenticationSignIn,
                async Task<IResult> (
                    [FromForm] SignInForm form,
                    HttpContext httpContext,
                    IdentityLoginGateway gateway,
                    OpenIdConnectEndpointResolver endpointResolver,
                    CancellationToken cancellationToken) =>
                {
                    string authorizationPath = await endpointResolver
                        .GetAuthorizationPathAsync(cancellationToken);
                    var returnUrl = NormalizeAuthorizeReturnUrl(
                        form.ReturnUrl,
                        httpContext.Request,
                        authorizationPath);
                    if (returnUrl is null)
                    {
                        return RedirectToLogin(
                            error: AuthenticationError.InvalidRequest,
                            returnUrl: null);
                    }

                    using var response = await gateway.SignInAsync(
                        form.Email,
                        form.Password,
                        returnUrl,
                        cancellationToken);

                    if (response.StatusCode != HttpStatusCode.Redirect)
                    {
                        return RedirectToLogin(
                            error: GetErrorCode(response.StatusCode),
                            returnUrl: returnUrl);
                    }

                    string[] setCookieHeaders = response.Headers.TryGetValues(
                        HeaderNames.SetCookie,
                        out var values)
                        ? [.. values]
                        : [];
                    if (setCookieHeaders.Length == 0)
                    {
                        return RedirectToLogin(
                            error: AuthenticationError.Unavailable,
                            returnUrl: returnUrl);
                    }

                    foreach (var setCookieHeader in setCookieHeaders)
                    {
                        httpContext.Response.Headers.Append(HeaderNames.SetCookie, setCookieHeader);
                    }

                    return TypedResults.LocalRedirect(returnUrl);
                })
            .AllowAnonymous();

        endpoints.MapPost(
            IdentityRoutes.AuthenticationLogout,
            ([FromForm] string? returnUrl) => TypedResults.SignOut(
                GetAuthenticationProperties(returnUrl),
                [
                    AuthenticationConstants.SessionScheme,
                    AuthenticationConstants.OpenIdConnectScheme,
                ]));

        return endpoints;
    }

    private static RedirectHttpResult RedirectToLogin(
        AuthenticationError error,
        string? returnUrl)
    {
        return TypedResults.Redirect(IdentityRoutes.LoginPage(returnUrl, error: error));
    }

    private static AuthenticationError GetErrorCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => AuthenticationError.InvalidCredentials,
            HttpStatusCode.Forbidden => AuthenticationError.Forbidden,
            HttpStatusCode.BadRequest => AuthenticationError.InvalidRequest,
            _ => AuthenticationError.Unavailable,
        };
    }

    private static string? NormalizeAuthorizeReturnUrl(
        string? returnUrl,
        HttpRequest request,
        string authorizationPath)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return null;
        }

        string pathAndQuery;
        if (returnUrl.StartsWith('/') &&
            !returnUrl.StartsWith("//", StringComparison.Ordinal))
        {
            pathAndQuery = returnUrl;
        }
        else if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri) &&
                 string.Equals(uri.Scheme, request.Scheme, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(uri.Authority, request.Host.Value, StringComparison.OrdinalIgnoreCase))
        {
            pathAndQuery = uri.PathAndQuery;
        }
        else
        {
            return null;
        }

        var queryStart = pathAndQuery.IndexOf('?');
        var path = queryStart < 0 ? pathAndQuery : pathAndQuery[..queryStart];

        return string.Equals(path, authorizationPath, StringComparison.Ordinal)
            ? pathAndQuery
            : null;
    }

    private static AuthenticationProperties GetAuthenticationProperties(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = AdminRoutes.Home;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (!returnUrl.StartsWith('/'))
        {
            returnUrl = $"/{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }

    internal sealed class SignInForm
    {
        public string Email { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;

        public string ReturnUrl { get; init; } = string.Empty;
    }
}
