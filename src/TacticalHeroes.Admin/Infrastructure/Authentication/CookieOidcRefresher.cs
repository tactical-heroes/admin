using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace TacticalHeroes.Admin.Infrastructure.Authentication;

internal sealed class CookieOidcRefresher(
    IOptionsMonitor<OpenIdConnectOptions> oidcOptionsMonitor)
{
    private readonly OpenIdConnectProtocolValidator _tokenValidator = new()
    {
        RequireNonce = false,
    };

    internal async Task ValidateOrRefreshCookieAsync(
        CookieValidatePrincipalContext context,
        string oidcScheme)
    {
        var expiresAtText = context.Properties.GetTokenValue("expires_at");
        if (!DateTimeOffset.TryParse(expiresAtText, out var expiresAt))
        {
            return;
        }

        var options = oidcOptionsMonitor.Get(oidcScheme);
        var now = options.TimeProvider!.GetUtcNow();
        if (now + TimeSpan.FromMinutes(5) < expiresAt)
        {
            return;
        }

        var configuration = await options.ConfigurationManager!.GetConfigurationAsync(
            context.HttpContext.RequestAborted);
        var tokenEndpoint = configuration.TokenEndpoint ??
            throw new InvalidOperationException("The OIDC token endpoint is missing.");
        var refreshToken = context.Properties.GetTokenValue("refresh_token");

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            context.RejectPrincipal();
            return;
        }

        var parameters = new Dictionary<string, string>
        {
            [OpenIdConnectParameterNames.GrantType] = OpenIdConnectGrantTypes.RefreshToken,
            [OpenIdConnectParameterNames.ClientId] = options.ClientId!,
            [OpenIdConnectParameterNames.Scope] = string.Join(' ', options.Scope),
            [OpenIdConnectParameterNames.RefreshToken] = refreshToken,
        };

        if (!string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            parameters[OpenIdConnectParameterNames.ClientSecret] = options.ClientSecret;
        }

        using var response = await options.Backchannel.PostAsync(
            tokenEndpoint,
            new FormUrlEncodedContent(parameters),
            context.HttpContext.RequestAborted);

        if (!response.IsSuccessStatusCode)
        {
            context.RejectPrincipal();
            return;
        }

        var responseJson = await response.Content.ReadAsStringAsync(
            context.HttpContext.RequestAborted);
        var message = new OpenIdConnectMessage(responseJson);
        var validationParameters = options.TokenValidationParameters.Clone();

        if (options.ConfigurationManager is BaseConfigurationManager baseConfigurationManager)
        {
            validationParameters.ConfigurationManager = baseConfigurationManager;
        }
        else
        {
            validationParameters.ValidIssuer = configuration.Issuer;
            validationParameters.IssuerSigningKeys = configuration.SigningKeys;
        }

        var validationResult = await options.TokenHandler.ValidateTokenAsync(
            message.IdToken,
            validationParameters);

        if (!validationResult.IsValid)
        {
            context.RejectPrincipal();
            return;
        }

        var validatedIdToken = JwtSecurityTokenConverter.Convert(
            validationResult.SecurityToken as JsonWebToken);
        validatedIdToken.Payload[System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Nonce] = null;
        _tokenValidator.ValidateTokenResponse(new OpenIdConnectProtocolValidationContext
        {
            ProtocolMessage = message,
            ClientId = options.ClientId,
            ValidatedIdToken = validatedIdToken,
        });

        context.ShouldRenew = true;
        context.ReplacePrincipal(new ClaimsPrincipal(validationResult.ClaimsIdentity));

        var expiresIn = int.Parse(
            message.ExpiresIn,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture);
        var refreshedExpiresAt = now + TimeSpan.FromSeconds(expiresIn);
        context.Properties.StoreTokens(
        [
            new AuthenticationToken { Name = "access_token", Value = message.AccessToken },
            new AuthenticationToken { Name = "id_token", Value = message.IdToken },
            new AuthenticationToken { Name = "refresh_token", Value = message.RefreshToken },
            new AuthenticationToken { Name = "token_type", Value = message.TokenType },
            new AuthenticationToken
            {
                Name = "expires_at",
                Value = refreshedExpiresAt.ToString("o", CultureInfo.InvariantCulture),
            },
        ]);
    }
}
