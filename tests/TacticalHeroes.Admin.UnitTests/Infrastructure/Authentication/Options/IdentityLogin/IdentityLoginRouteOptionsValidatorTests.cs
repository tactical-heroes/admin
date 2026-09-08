using TacticalHeroes.Admin.Infrastructure.Authentication.Options.IdentityLogin;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.Options.IdentityLogin;

public sealed class IdentityLoginRouteOptionsValidatorTests
{
    [Theory(DisplayName = "Validate should reject invalid paths when login path is not application local")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("login")]
    [InlineData("https://identity.example.test/login")]
    [InlineData("//identity.example.test/login")]
    public void Validate_Should_RejectInvalidPaths_When_LoginPathIsNotApplicationLocal(string path)
    {
        var validator = new IdentityLoginRouteOptionsValidator();
        var options = new IdentityLoginRouteOptions { Path = path };

        var result = validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe([
            "ReverseProxy:Routes:tactical-heroes-auth-login:Match:Path must be an absolute application path."
        ]);
    }

    [Fact(DisplayName = "Validate should accept login route when path is application local")]
    public void Validate_Should_AcceptLoginRoute_When_PathIsApplicationLocal()
    {
        var validator = new IdentityLoginRouteOptionsValidator();
        var options = new IdentityLoginRouteOptions { Path = "/identity/login" };

        var result = validator.Validate(null, options);

        result.Succeeded.ShouldBeTrue();
    }
}
