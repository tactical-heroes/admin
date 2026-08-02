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

using TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;

namespace TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;

internal sealed class CookieOidcRefresher(
    IOptionsMonitor<OpenIdConnectOptions> oidcOptionsMonitor,
    IOptions<AdminOpenIdConnectOptions> configuredOptions)
{
    private const string ExpiresAtTokenName = "expires_at";
    private const string RoundtripDateTimeFormat = "O";

    private readonly OpenIdConnectProtocolValidator _tokenValidator = new()
    {
        RequireNonce = false,
    };

    internal async Task ValidateOrRefreshCookieAsync(
        CookieValidatePrincipalContext context,
        string oidcScheme)
    {
        var expiresAtText = context.Properties.GetTokenValue(ExpiresAtTokenName);
        if (!DateTimeOffset.TryParse(expiresAtText, out var expiresAt))
        {
            return;
        }

        var options = oidcOptionsMonitor.Get(oidcScheme);
        var now = options.TimeProvider!.GetUtcNow();
        if (now + configuredOptions.Value.RefreshBeforeExpiration < expiresAt)
        {
            return;
        }

        var configuration = await options.ConfigurationManager!.GetConfigurationAsync(
            context.HttpContext.RequestAborted);
        var tokenEndpoint = configuration.TokenEndpoint ??
            throw new InvalidOperationException("The OIDC token endpoint is missing.");
        var refreshToken = context.Properties.GetTokenValue(
            OpenIdConnectParameterNames.RefreshToken);

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
        validatedIdToken.Payload[
            System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Nonce] = null;
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
            new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = message.AccessToken,
            },
            new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.IdToken,
                Value = message.IdToken,
            },
            new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.RefreshToken,
                Value = message.RefreshToken,
            },
            new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.TokenType,
                Value = message.TokenType,
            },
            new AuthenticationToken
            {
                Name = ExpiresAtTokenName,
                Value = refreshedExpiresAt.ToString(
                    RoundtripDateTimeFormat,
                    CultureInfo.InvariantCulture),
            },
        ]);
    }
}
