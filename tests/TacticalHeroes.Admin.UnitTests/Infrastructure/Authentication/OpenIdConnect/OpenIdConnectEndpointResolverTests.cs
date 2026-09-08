using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using TacticalHeroes.Admin.Infrastructure.Authentication;
using TacticalHeroes.Admin.Infrastructure.Authentication.OpenIdConnect;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.OpenIdConnect;

public sealed class OpenIdConnectEndpointResolverTests
{
    [Theory(DisplayName = "GetAuthorizationPathAsync should extract path when metadata endpoint is absolute")]
    [InlineData("https://identity.example.test/connect/authorize", "/connect/authorize")]
    [InlineData("https://identity.example.test/tenant/authorize?client_id=admin", "/tenant/authorize")]
    [InlineData("http://localhost:5000/connect/authorize", "/connect/authorize")]
    public async Task GetAuthorizationPathAsync_Should_ExtractPath_When_MetadataEndpointIsAbsolute(
        string endpoint,
        string expectedPath)
    {
        var configuration = new OpenIdConnectConfiguration { AuthorizationEndpoint = endpoint };
        using var services = CreateServices(new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration));
        var resolver = new OpenIdConnectEndpointResolver(services.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>());

        string path = await resolver.GetAuthorizationPathAsync(TestContext.Current.CancellationToken);

        path.ShouldBe(expectedPath);
    }

    [Theory(DisplayName = "GetAuthorizationPathAsync should reject metadata when authorization endpoint is invalid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/connect/authorize")]
    [InlineData("connect/authorize")]
    [InlineData("file:///connect/authorize")]
    [InlineData("ftp://identity.example.test/connect/authorize")]
    public async Task GetAuthorizationPathAsync_Should_RejectMetadata_When_AuthorizationEndpointIsInvalid(string? endpoint)
    {
        var configuration = new OpenIdConnectConfiguration { AuthorizationEndpoint = endpoint };
        using var services = CreateServices(new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration));
        var resolver = new OpenIdConnectEndpointResolver(services.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>());

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => resolver.GetAuthorizationPathAsync(TestContext.Current.CancellationToken));

        exception.Message.ShouldBe("The OIDC authorization endpoint is missing or invalid.");
    }

    [Fact(DisplayName = "GetAuthorizationPathAsync should reject configuration when metadata manager is missing")]
    public async Task GetAuthorizationPathAsync_Should_RejectConfiguration_When_MetadataManagerIsMissing()
    {
        using var services = CreateServices(null);
        var resolver = new OpenIdConnectEndpointResolver(services.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>());

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => resolver.GetAuthorizationPathAsync(TestContext.Current.CancellationToken));

        exception.Message.ShouldBe("The OIDC configuration manager is missing.");
    }

    private static ServiceProvider CreateServices(IConfigurationManager<OpenIdConnectConfiguration>? configurationManager)
    {
        var services = new ServiceCollection();
        services.AddOptions<OpenIdConnectOptions>(AuthenticationConstants.OpenIdConnectScheme)
            .Configure(options => options.ConfigurationManager = configurationManager);
        return services.BuildServiceProvider();
    }
}
