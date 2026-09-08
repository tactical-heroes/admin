using Microsoft.Extensions.Configuration;

using TacticalHeroes.Admin.Infrastructure.Authentication.Options.OpenIdConnect;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.Options.OpenIdConnect;

public sealed class AdminOpenIdConnectOptionsValidatorTests
{
    [Theory(DisplayName = "Validate should accept settings when authority uses an http scheme")]
    [InlineData("https://identity.example.test")]
    [InlineData("http://localhost:5000")]
    public void Validate_Should_AcceptSettings_When_AuthorityUsesAnHttpScheme(string authority)
    {
        var settings = CreateSettings();
        settings["Authority"] = authority;
        var validator = new AdminOpenIdConnectOptionsValidator();
        var options = BindSettings(settings);

        var result = validator.Validate(null, options);

        result.Succeeded.ShouldBeTrue();
    }

    [Theory(DisplayName = "Validate should report the invalid setting when one option is invalid")]
    [InlineData("Authority", "", "must be an absolute HTTP or HTTPS URI.")]
    [InlineData("Authority", "relative/path", "must be an absolute HTTP or HTTPS URI.")]
    [InlineData("Authority", "ftp://identity.example.test", "must be an absolute HTTP or HTTPS URI.")]
    [InlineData("ClientId", " ", "is required.")]
    [InlineData("CallbackPath", "", "must be an absolute application path.")]
    [InlineData("CallbackPath", "signin-oidc", "must be an absolute application path.")]
    [InlineData("CallbackPath", "//identity.example.test/signin", "must be an absolute application path.")]
    [InlineData("SignedOutCallbackPath", " ", "must be an absolute application path.")]
    [InlineData("SignedOutCallbackPath", "signout-callback-oidc", "must be an absolute application path.")]
    [InlineData("SignedOutCallbackPath", "//identity.example.test/signout", "must be an absolute application path.")]
    [InlineData("RefreshBeforeExpiration", "00:00:00", "must be positive.")]
    [InlineData("RefreshBeforeExpiration", "-00:00:01", "must be positive.")]
    [InlineData("NameClaimType", " ", "is required.")]
    [InlineData("RoleClaimType", "", "is required.")]
    public void Validate_Should_ReportTheInvalidSetting_When_OneOptionIsInvalid(
        string setting,
        string value,
        string failure)
    {
        var settings = CreateSettings();
        settings[setting] = value;
        var validator = new AdminOpenIdConnectOptionsValidator();
        var options = BindSettings(settings);

        var result = validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe([$"Authentication:OpenIdConnect:{setting} {failure}"]);
    }

    [Theory(DisplayName = "Validate should reject scopes when required scopes are invalid")]
    [InlineData("", "Scopes must contain only non-empty values.")]
    [InlineData("openid, ", "Scopes must contain only non-empty values.")]
    [InlineData("openid,offline_access,openid", "Scopes must not contain duplicates.")]
    [InlineData("offline_access", "Scopes must contain 'openid'.")]
    [InlineData("openid", "Scopes must contain 'offline_access'.")]
    [InlineData("OpenId,offline_access", "Scopes must contain 'openid'.")]
    public void Validate_Should_RejectScopes_When_RequiredScopesAreInvalid(string scopes, string failure)
    {
        var settings = CreateSettings();
        settings.Remove("Scopes:0");
        settings.Remove("Scopes:1");
        string[] values = scopes.Length == 0 ? [] : scopes.Split(',');
        for (int index = 0; index < values.Length; index++)
        {
            settings[$"Scopes:{index}"] = values[index];
        }

        var validator = new AdminOpenIdConnectOptionsValidator();
        var options = BindSettings(settings);

        var result = validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe([$"Authentication:OpenIdConnect:{failure}"]);
    }

    [Fact(DisplayName = "Validate should report all failures when oidc settings are missing")]
    public void Validate_Should_ReportAllFailures_When_OidcSettingsAreMissing()
    {
        var validator = new AdminOpenIdConnectOptionsValidator();

        var result = validator.Validate(null, new AdminOpenIdConnectOptions());

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe([
            "Authentication:OpenIdConnect:Authority must be an absolute HTTP or HTTPS URI.",
            "Authentication:OpenIdConnect:ClientId is required.",
            "Authentication:OpenIdConnect:CallbackPath must be an absolute application path.",
            "Authentication:OpenIdConnect:SignedOutCallbackPath must be an absolute application path.",
            "Authentication:OpenIdConnect:RefreshBeforeExpiration must be positive.",
            "Authentication:OpenIdConnect:NameClaimType is required.",
            "Authentication:OpenIdConnect:RoleClaimType is required.",
            "Authentication:OpenIdConnect:Scopes must contain only non-empty values."
        ], ignoreOrder: true);
    }

    private static Dictionary<string, string?> CreateSettings()
    {
        return new Dictionary<string, string?>
        {
            ["Authority"] = "https://identity.example.test",
            ["ClientId"] = "admin",
            ["CallbackPath"] = "/signin-oidc",
            ["SignedOutCallbackPath"] = "/signout-callback-oidc",
            ["RefreshBeforeExpiration"] = "00:05:00",
            ["NameClaimType"] = "name",
            ["RoleClaimType"] = "role",
            ["Scopes:0"] = "openid",
            ["Scopes:1"] = "offline_access"
        };
    }

    private static AdminOpenIdConnectOptions BindSettings(Dictionary<string, string?> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build()
            .Get<AdminOpenIdConnectOptions>()!;
    }
}
