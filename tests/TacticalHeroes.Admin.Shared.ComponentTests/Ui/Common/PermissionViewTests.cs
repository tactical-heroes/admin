using System.Security.Claims;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Common;

public sealed class PermissionViewTests : BunitContext
{
    [Fact(DisplayName = "OnParametersSetAsync should update visibility when permissions change")]
    public void OnParametersSetAsync_Should_UpdateVisibility_When_PermissionsChange()
    {
        var authorization = AddAuthorization();
        authorization.SetAuthorized("Editor");
        authorization.SetClaims(new Claim("permission", "identity.users.read"));
        var component = Render<PermissionView>(parameters => parameters
            .Add(view => view.Permissions, ["identity.users.read", "identity.users.update"])
            .Add(view => view.ChildContent, "Edit user"));
        component.Markup.ShouldBeEmpty();

        authorization.SetClaims(
            new Claim("permission", "identity.users.read"), new Claim("permission", "identity.users.update"));

        component.WaitForAssertion(() => component.Markup.ShouldBe("Edit user"));
        authorization.SetNotAuthorized();
        component.WaitForAssertion(() => component.Markup.ShouldBeEmpty());
    }
}
