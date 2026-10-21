using MudBlazor;
using MudBlazor.Extensions;

using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Entities.Roles.Ui;

public sealed class RoleSelectTests : IdentityCatalogTestContext
{
    [Fact(DisplayName = "OnInitializedAsync should load options in one request when more than one list page exists")]
    public void OnInitializedAsync_Should_LoadOptionsInOneRequest_When_MoreThanOneListPageExists()
    {
        Handler.Roles = [.. Enumerable.Range(0, 101).Select(index => new RoleOption
        {
            Id = Guid.CreateVersion7(),
            Name = $"Role {index:D3}"
        })];

        var component = Render<RoleSelect>(parameters => parameters.Add(select => select.RoleIds, []));

        component.WaitForAssertion(() => component.Instance.Items!.Count.ShouldBe(101));
        var request = Handler.Requests.ShouldHaveSingleItem();
        request.AbsolutePath.ShouldBe("/api/v1/roles/select-options");
        request.Query.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetRoleName should indicate default role when selected role is default")]
    public void GetRoleName_Should_IndicateDefaultRole_When_SelectedRoleIsDefault()
    {
        Handler.Roles = [new RoleOption { Id = IdentityCatalogResponses.RoleId, Name = "Player", IsDefault = true }];

        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.RoleIds, [IdentityCatalogResponses.RoleId]));

        component.Find("input").GetAttribute("value").ShouldBe("Player (по умолчанию)");
    }

    [Fact(DisplayName = "LoadAsync should retain selected roles and retry when catalog fails")]
    public void LoadAsync_Should_RetainSelectedRolesAndRetry_When_CatalogFails()
    {
        Handler.Fail = true;
        List<Guid> roles = [IdentityCatalogResponses.RoleId];
        var component = Render<RoleSelect>(parameters => parameters.Add(select => select.RoleIds, roles));
        component.WaitForElement(".mud-alert");
        roles.ShouldBe([IdentityCatalogResponses.RoleId]);
        Handler.Fail = false;

        component.Find("button").Click();

        component.WaitForAssertion(() => component.FindComponent<MudSelect<Guid>>()
            .Instance.GetState(select => select.SelectedValues).ShouldBe(roles));
        roles.ShouldBe([IdentityCatalogResponses.RoleId]);
    }

    [Theory(DisplayName = "UpdateRoles should update selection when roles are selected or cleared")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateRoles_Should_UpdateSelection_When_RolesAreSelectedOrCleared(bool selected)
    {
        List<Guid> roles = [Guid.Parse("2994d18b-f957-4a09-a366-ac1606c31018")];
        var component = Render<RoleSelect>(parameters => parameters.Add(select => select.RoleIds, roles));
        Guid[] values = selected ? [IdentityCatalogResponses.RoleId] : [];

        await component.InvokeAsync(() => component.FindComponent<MudSelect<Guid>>()
            .Instance.SelectedValuesChanged.InvokeAsync(values));

        roles.ShouldBe(values);
    }

    [Fact(DisplayName = "GetRoleName should retain missing role when selected role is absent from catalog")]
    public void GetRoleName_Should_RetainMissingRole_When_SelectedRoleIsAbsentFromCatalog()
    {
        Guid missingId = Guid.Parse("2994d18b-f957-4a09-a366-ac1606c31018");
        List<Guid> roles = [missingId];

        var component = Render<RoleSelect>(parameters => parameters.Add(select => select.RoleIds, roles));

        component.FindComponents<MudSelectItem<Guid>>().ShouldContain(item => item.Instance.Value == missingId);
        component.Find("input").GetAttribute("value")!.ShouldContain("Недоступная роль");
        roles.ShouldBe([missingId]);
    }

    [Fact(DisplayName = "GetRoleName should display role name when role is selected")]
    public void GetRoleName_Should_DisplayRoleName_When_RoleIsSelected()
    {
        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.RoleIds, [IdentityCatalogResponses.RoleId]));

        component.Find("input").GetAttribute("value").ShouldBe("Moderator");
    }
}
