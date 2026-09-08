using TacticalHeroes.Admin.Infrastructure.Authentication.Options.Session;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.Options.Session;

public sealed class AdminSessionOptionsValidatorTests
{
    [Theory(DisplayName = "Validate should reject cookie names when cookie name is invalid")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid name")]
    [InlineData("invalid;name")]
    public void Validate_Should_RejectCookieNames_When_CookieNameIsInvalid(string cookieName)
    {
        var validator = new AdminSessionOptionsValidator();
        var options = new AdminSessionOptions
        {
            CookieName = cookieName,
            Lifetime = TimeSpan.FromHours(1)
        };

        var result = validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe(["Authentication:Session:CookieName must be a valid cookie name."]);
    }

    [Theory(DisplayName = "Validate should reject lifetime when duration is not positive")]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_Should_RejectLifetime_When_DurationIsNotPositive(int seconds)
    {
        var validator = new AdminSessionOptionsValidator();
        var options = new AdminSessionOptions
        {
            CookieName = "__Host-AdminSession",
            Lifetime = TimeSpan.FromSeconds(seconds)
        };

        var result = validator.Validate(null, options);

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe(["Authentication:Session:Lifetime must be positive."]);
    }

    [Fact(DisplayName = "Validate should report all failures when session settings are missing")]
    public void Validate_Should_ReportAllFailures_When_SessionSettingsAreMissing()
    {
        var validator = new AdminSessionOptionsValidator();

        var result = validator.Validate(null, new AdminSessionOptions());

        result.Failed.ShouldBeTrue();
        result.Failures.ShouldBe([
            "Authentication:Session:CookieName must be a valid cookie name.",
            "Authentication:Session:Lifetime must be positive."
        ], ignoreOrder: true);
    }

    [Fact(DisplayName = "Validate should accept session when cookie and lifetime are valid")]
    public void Validate_Should_AcceptSession_When_CookieAndLifetimeAreValid()
    {
        var validator = new AdminSessionOptionsValidator();
        var options = new AdminSessionOptions
        {
            CookieName = "__Host-AdminSession",
            Lifetime = TimeSpan.FromHours(1)
        };

        var result = validator.Validate(null, options);

        result.Succeeded.ShouldBeTrue();
    }
}
