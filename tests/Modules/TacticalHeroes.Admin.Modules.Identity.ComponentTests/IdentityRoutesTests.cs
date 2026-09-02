using TacticalHeroes.Admin.Modules.Identity.Entities.Authentication.Model;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests;

public sealed class IdentityRoutesTests
{
    [Fact(DisplayName = "Builds role route from identifier")]
    public void Role_Should_BuildRoute_When_IdentifierIsProvided()
    {
        var id = Guid.Parse("bde93e9c-b7b9-4647-a04a-6e58b0290082");

        string route = IdentityRoutes.Role(id);

        route.ShouldBe("/roles/bde93e9c-b7b9-4647-a04a-6e58b0290082");
    }

    [Fact(DisplayName = "Builds login route from typed query parameters")]
    public void LoginPage_Should_EncodeQuery_When_TypedParametersAreProvided()
    {
        string route = IdentityRoutes.LoginPage(
            "/connect/authorize?client_id=admin",
            LoginMode.Register,
            AuthenticationError.InvalidRequest);

        route.ShouldBe(
            "/login?mode=register" +
            "&returnUrl=%2Fconnect%2Fauthorize%3Fclient_id%3Dadmin" +
            "&error=invalid_request");
    }

    [Fact(DisplayName = "Builds OAuth error route with its configured enum name")]
    public void LoginPage_Should_UseConfiguredName_When_OAuthErrorIsProvided()
    {
        string route = IdentityRoutes.LoginPage(error: AuthenticationError.OAuth);

        route.ShouldBe("/login?error=oauth");
    }

    [Fact(DisplayName = "Builds confirmation route from typed parameters")]
    public void ConfirmEmailPage_Should_EncodeToken_When_ParametersAreProvided()
    {
        var userId = Guid.Parse("bc49d005-4cbc-4941-985d-1354cb6c68d3");

        string route = IdentityRoutes.ConfirmEmailPage(userId, "token/+==");

        route.ShouldBe(
            "/confirm-email?userId=bc49d005-4cbc-4941-985d-1354cb6c68d3" +
            "&emailConfirmationToken=token%2F%2B%3D%3D");
    }
}
