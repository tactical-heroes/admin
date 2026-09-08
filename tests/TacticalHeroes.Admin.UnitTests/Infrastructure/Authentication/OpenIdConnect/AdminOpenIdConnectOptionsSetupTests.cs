using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;
using TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.OpenIdConnect;

public sealed class AdminOpenIdConnectOptionsSetupTests
{
    [Theory(DisplayName = "Configure should apply oidc settings when admin scheme is configured")]
    [InlineData(true)]
    [InlineData(false)]
    public void Configure_Should_ApplyOidcSettings_When_AdminSchemeIsConfigured(bool requireHttpsMetadata)
    {
        var setup = CreateSetup(requireHttpsMetadata);
        var options = new OpenIdConnectOptions();

        setup.Configure(AuthenticationConstants.OpenIdConnectScheme, options);

        options.Authority.ShouldBe("https://identity.example.test");
        options.ClientId.ShouldBe("admin");
        options.SignInScheme.ShouldBe(AuthenticationConstants.SessionScheme);
        options.CallbackPath.Value.ShouldBe("/signin-oidc");
        options.SignedOutCallbackPath.Value.ShouldBe("/signout-callback-oidc");
        options.SignedOutRedirectUri.ShouldBe("/");
        options.RequireHttpsMetadata.ShouldBe(requireHttpsMetadata);
        options.ResponseType.ShouldBe(OpenIdConnectResponseType.Code);
        options.UsePkce.ShouldBeTrue();
        options.PushedAuthorizationBehavior.ShouldBe(PushedAuthorizationBehavior.Require);
        options.MapInboundClaims.ShouldBeFalse();
        options.GetClaimsFromUserInfoEndpoint.ShouldBeTrue();
        options.SaveTokens.ShouldBeTrue();
        options.TokenValidationParameters.NameClaimType.ShouldBe("name");
        options.TokenValidationParameters.RoleClaimType.ShouldBe("role");
        options.Scope.ShouldBe(["openid", "offline_access", "admin-api"], ignoreOrder: true);
    }

    [Theory(DisplayName = "Configure should preserve options when scheme is unrelated")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("other-oidc")]
    public void Configure_Should_PreserveOptions_When_SchemeIsUnrelated(string? scheme)
    {
        var setup = CreateSetup();
        var options = new OpenIdConnectOptions { Authority = "https://other.example.test" };
        var events = options.Events;

        setup.Configure(scheme, options);

        options.Authority.ShouldBe("https://other.example.test");
        options.Events.ShouldBeSameAs(events);
    }

    [Fact(DisplayName = "Configure should preserve options when no scheme is specified")]
    public void Configure_Should_PreserveOptions_When_NoSchemeIsSpecified()
    {
        var setup = CreateSetup();
        var options = new OpenIdConnectOptions { ClientId = "other-client" };
        var events = options.Events;

        setup.Configure(options);

        options.ClientId.ShouldBe("other-client");
        options.Events.ShouldBeSameAs(events);
    }

    [Theory(DisplayName = "Redirect should use public origin when provider endpoint is absolute")]
    [InlineData(false, "https", "admin.example.test", "", "https://admin.example.test/connect/authorize?request_uri=urn%3Arequest")]
    [InlineData(true, "https", "admin.example.test:8443", "/admin", "https://admin.example.test:8443/admin/connect/authorize?request_uri=urn%3Arequest")]
    [InlineData(false, "http", "localhost:5000", "/admin", "http://localhost:5000/admin/connect/authorize?request_uri=urn%3Arequest")]
    public async Task Redirect_Should_UsePublicOrigin_When_ProviderEndpointIsAbsolute(
        bool signOut,
        string scheme,
        string host,
        string pathBase,
        string expectedAddress)
    {
        var options = CreateOptions();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = scheme;
        httpContext.Request.Host = new HostString(host);
        httpContext.Request.PathBase = pathBase;
        var context = new RedirectContext(httpContext, CreateScheme(), options, new AuthenticationProperties())
        {
            ProtocolMessage = new OpenIdConnectMessage
            {
                IssuerAddress = "https://identity.internal/connect/authorize?request_uri=urn%3Arequest"
            }
        };

        await (signOut
            ? options.Events.RedirectToIdentityProviderForSignOut(context)
            : options.Events.RedirectToIdentityProvider(context));

        context.ProtocolMessage.IssuerAddress.ShouldBe(expectedAddress);
    }

    [Theory(DisplayName = "Redirect should reject endpoint when provider address is invalid")]
    [InlineData(false, "")]
    [InlineData(false, "/connect/authorize")]
    [InlineData(true, "")]
    [InlineData(true, "/connect/logout")]
    public async Task Redirect_Should_RejectEndpoint_When_ProviderAddressIsInvalid(bool signOut, string address)
    {
        var options = CreateOptions();
        var context = new RedirectContext(new DefaultHttpContext(), CreateScheme(), options, new AuthenticationProperties())
        {
            ProtocolMessage = new OpenIdConnectMessage { IssuerAddress = address }
        };

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => signOut
            ? options.Events.RedirectToIdentityProviderForSignOut(context)
            : options.Events.RedirectToIdentityProvider(context));

        exception.Message.ShouldBe("The OIDC endpoint address is missing or invalid.");
    }

    [Fact(DisplayName = "RemoteFailure should redirect to login when provider authentication fails")]
    public async Task RemoteFailure_Should_RedirectToLogin_When_ProviderAuthenticationFails()
    {
        var options = CreateOptions();
        var context = new RemoteFailureContext(
            new DefaultHttpContext(), CreateScheme(), options, new InvalidOperationException("Provider failure"));

        await options.Events.RemoteFailure(context);

        context.Result.ShouldNotBeNull().Handled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status302Found);
        context.Response.Headers.Location.ToString().ShouldBe("/login?error=oauth");
    }

    private static AdminOpenIdConnectOptionsSetup CreateSetup(bool requireHttpsMetadata = true)
    {
        return new AdminOpenIdConnectOptionsSetup(Microsoft.Extensions.Options.Options.Create(new AdminOpenIdConnectOptions
        {
            Authority = "https://identity.example.test/",
            ClientId = "admin",
            CallbackPath = "/signin-oidc",
            SignedOutCallbackPath = "/signout-callback-oidc",
            RequireHttpsMetadata = requireHttpsMetadata,
            NameClaimType = "name",
            RoleClaimType = "role",
            Scopes = ["openid", "offline_access", "admin-api"]
        }));
    }

    private static OpenIdConnectOptions CreateOptions()
    {
        var options = new OpenIdConnectOptions();
        CreateSetup().Configure(AuthenticationConstants.OpenIdConnectScheme, options);
        return options;
    }

    private static AuthenticationScheme CreateScheme()
    {
        return new AuthenticationScheme(AuthenticationConstants.OpenIdConnectScheme, null, typeof(OpenIdConnectHandler));
    }
}
