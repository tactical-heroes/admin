using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Shared.Authorization;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Authorization;

public sealed class PermissionAccessTests : BunitContext
{
    [Fact(DisplayName = "AuthorizeAsync should require exact permission when policy is evaluated")]
    public async Task AuthorizeAsync_Should_RequireExactPermission_When_PolicyIsEvaluated()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPermissionPolicies(["identity.users.update"]);
        await using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IAuthorizationService>();
        var granted = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("permission", "identity.users.update")], "Test"));
        var denied = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("permission", "identity.users.read"), new Claim("section", "identity")], "Test"));

        (await service.AuthorizeAsync(granted, null, "identity.users.update")).Succeeded.ShouldBeTrue();
        (await service.AuthorizeAsync(denied, null, "identity.users.update")).Succeeded.ShouldBeFalse();
        (await service.AuthorizeAsync(new ClaimsPrincipal(), null, "identity.users.update")).Succeeded.ShouldBeFalse();
    }
}
