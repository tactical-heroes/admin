using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

using TacticalHeroes.Admin.Client.App.Routing;
using TacticalHeroes.Admin.Infrastructure.Authentication.Login;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Modules.Identity;

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
                        return RedirectToLogin(error: LoginError.InvalidRequest, returnUrl: null);
                    }

                    var result = await gateway.SignInAsync(
                        form.Email,
                        form.Password,
                        returnUrl,
                        cancellationToken);

                    if (result.Status != IdentityLoginStatus.Succeeded)
                    {
                        return RedirectToLogin(
                            error: GetErrorCode(result.Status),
                            returnUrl: returnUrl);
                    }

                    foreach (var setCookieHeader in result.SetCookieHeaders)
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

    private static IResult RedirectToLogin(LoginError error, string? returnUrl)
    {
        return TypedResults.Redirect(IdentityRoutes.LoginPage(returnUrl, error: error));
    }

    private static LoginError GetErrorCode(IdentityLoginStatus status)
    {
        return status switch
        {
            IdentityLoginStatus.InvalidCredentials => LoginError.InvalidCredentials,
            IdentityLoginStatus.Forbidden => LoginError.Forbidden,
            IdentityLoginStatus.InvalidRequest => LoginError.InvalidRequest,
            _ => LoginError.Unavailable,
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
