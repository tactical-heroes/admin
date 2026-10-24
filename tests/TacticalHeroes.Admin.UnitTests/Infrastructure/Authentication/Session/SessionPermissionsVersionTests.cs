using System.Security.Claims;

using TacticalHeroes.Admin.Infrastructure.Authentication.Session;

namespace TacticalHeroes.Admin.UnitTests.Infrastructure.Authentication.Session;

public sealed class SessionPermissionsVersionTests
{
    [Fact(DisplayName = "Create should preserve version when tokens rotate and effective permissions stay the same")]
    public void Create_Should_PreserveVersion_When_TokensRotateAndEffectivePermissionsStayTheSame()
    {
        var first = CreateUser("user", "compendium.factions.read", "compendium.heroes.read");
        var second = CreateUser("user", "compendium.heroes.read", "compendium.factions.read", "compendium.factions.read");
        second.Identities.Single().AddClaim(new Claim("exp", "123456"));

        SessionPermissionsVersion.Create(second).ShouldBe(SessionPermissionsVersion.Create(first));
    }

    [Theory(DisplayName = "Create should change version when user or effective permissions change")]
    [InlineData("other", "compendium.factions.read")]
    [InlineData("user", "compendium.heroes.read")]
    [InlineData("user", null)]
    public void Create_Should_ChangeVersion_When_UserOrEffectivePermissionsChange(string subject, string? permission)
    {
        var first = CreateUser("user", "compendium.factions.read");
        var second = CreateUser(subject, permission is null ? [] : [permission]);

        SessionPermissionsVersion.Create(second).ShouldNotBe(SessionPermissionsVersion.Create(first));
    }

    private static ClaimsPrincipal CreateUser(string subject, params string[] permissions) =>
        new(new ClaimsIdentity([new Claim("sub", subject), .. permissions.Select(value => new Claim("permission", value))], "session"));
}
