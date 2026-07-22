using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace TacticalHeroes.Admin.Infrastructure.Authentication;

internal static class AuthenticationEndpointRouteBuilderExtensions
{
    internal static IEndpointRouteBuilder MapAdminAuthentication(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/authentication");

        group.MapGet(
                "/challenge",
                (string? returnUrl) => TypedResults.Challenge(GetAuthenticationProperties(returnUrl)))
            .AllowAnonymous();

        group.MapPost(
                "/sign-in",
                async Task<IResult> (
                    [FromForm] SignInForm form,
                    HttpContext httpContext,
                    IdentityLoginGateway gateway,
                    CancellationToken cancellationToken) =>
                {
                    var returnUrl = NormalizeAuthorizeReturnUrl(form.ReturnUrl, httpContext.Request);
                    if (returnUrl is null)
                    {
                        return RedirectToLogin(error: "invalid_request", returnUrl: null);
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

        group.MapPost(
            "/logout",
            ([FromForm] string? returnUrl) => TypedResults.SignOut(
                GetAuthenticationProperties(returnUrl),
                [
                    AuthenticationConstants.SessionScheme,
                    AuthenticationConstants.OpenIdConnectScheme,
                ]));

        return endpoints;
    }

    private static IResult RedirectToLogin(string error, string? returnUrl)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["error"] = error,
            ["returnUrl"] = returnUrl,
        };

        return TypedResults.Redirect(QueryHelpers.AddQueryString("/login", parameters));
    }

    private static string GetErrorCode(IdentityLoginStatus status)
    {
        return status switch
        {
            IdentityLoginStatus.InvalidCredentials => "invalid_credentials",
            IdentityLoginStatus.Forbidden => "forbidden",
            IdentityLoginStatus.InvalidRequest => "invalid_request",
            _ => "unavailable",
        };
    }

    private static string? NormalizeAuthorizeReturnUrl(string? returnUrl, HttpRequest request)
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

        return string.Equals(path, "/connect/authorize", StringComparison.Ordinal)
            ? pathAndQuery
            : null;
    }

    private static AuthenticationProperties GetAuthenticationProperties(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = "/";
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
