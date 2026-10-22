using Microsoft.AspNetCore.Components.Web;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Entities.Roles.Ui;

public sealed class RoleSelectTests : IdentityCatalogTestContext
{
    [Fact(DisplayName = "OnParametersSetAsync should load complete catalog when many roles exist")]
    public async Task OnParametersSetAsync_Should_LoadCompleteCatalog_When_ManyRolesExist()
    {
        Handler.Roles = [.. Enumerable.Range(0, 101).Select(index => new RoleOption
        {
            Id = Guid.CreateVersion7(),
            Name = $"Role {index:D3}"
        })];
        var lastRole = Handler.Roles[^1];
        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.MultiSelection, true)
            .Add(select => select.Values, [lastRole.Id]));

        await component.Find("fieldset").ClickAsync(new MouseEventArgs());

        Popovers.FindAll("[role=menuitemcheckbox]").Count.ShouldBe(101);
        component.Find(".mud-chip-content").TextContent.Trim().ShouldBe(lastRole.Name);
        var request = Handler.Requests.ShouldHaveSingleItem();
        request.AbsolutePath.ShouldBe("/api/v1/roles/select-options");
        request.Query.ShouldBeEmpty();
    }

    [Fact(DisplayName = "GetOptionName should indicate default role when selected role is default")]
    public void GetOptionName_Should_IndicateDefaultRole_When_SelectedRoleIsDefault()
    {
        Handler.Roles = [new RoleOption { Id = IdentityCatalogResponses.RoleId, Name = "Player", IsDefault = true }];

        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.MultiSelection, true)
            .Add(select => select.Values, [IdentityCatalogResponses.RoleId]));

        component.Find(".mud-chip-content").TextContent.Trim().ShouldBe("Player (по умолчанию)");
    }

    [Fact(DisplayName = "LoadAsync should retain selected roles and retry when catalog fails")]
    public void LoadAsync_Should_RetainSelectedRolesAndRetry_When_CatalogFails()
    {
        Handler.Fail = true;
        List<Guid> roles = [IdentityCatalogResponses.RoleId];
        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.MultiSelection, true)
            .Add(select => select.Values, roles));
        component.WaitForElement(".mud-alert");
        roles.ShouldBe([IdentityCatalogResponses.RoleId]);
        Handler.Fail = false;

        component.Find("button").Click();

        component.WaitForAssertion(() => component.Find(".mud-chip-content").TextContent.Trim().ShouldBe("Moderator"));
        roles.ShouldBe([IdentityCatalogResponses.RoleId]);
        Handler.Requests.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "Render should select and deselect roles when options are toggled")]
    public async Task Render_Should_SelectAndDeselectRoles_When_OptionsAreToggled()
    {
        List<Guid> roles = [];
        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.MultiSelection, true)
            .Add(select => select.Values, roles)
            .Add(select => select.ValuesChanged, values => roles = values));
        await component.Find("fieldset").ClickAsync(new MouseEventArgs());

        await Popovers.FindAll("[role=menuitemcheckbox]").Single(item => item.TextContent.Trim() == "Moderator")
            .ClickAsync(new MouseEventArgs());

        roles.ShouldBe([IdentityCatalogResponses.RoleId]);
        component.Find(".mud-chip-content").TextContent.Trim().ShouldBe("Moderator");
        await Popovers.FindAll("[role=menuitemcheckbox]").Single(item => item.TextContent.Trim() == "Moderator")
            .ClickAsync(new MouseEventArgs());
        roles.ShouldBeEmpty();
        component.FindComponents<MudChip<Guid>>().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Render should retain deselectable missing role when selected role is absent from catalog")]
    public async Task Render_Should_RetainDeselectableMissingRole_When_SelectedRoleIsAbsentFromCatalog()
    {
        Guid missingId = Guid.Parse("2994d18b-f957-4a09-a366-ac1606c31018");
        List<Guid> roles = [missingId];
        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.MultiSelection, true)
            .Add(select => select.Values, roles)
            .Add(select => select.ValuesChanged, values => roles = values));
        component.Find(".mud-chip-content").TextContent.ShouldContain("Недоступный вариант");
        roles.ShouldBe([missingId]);
        await component.Find("fieldset").ClickAsync(new MouseEventArgs());

        await Popovers.FindAll("[role=menuitemcheckbox]").Single(item => item.TextContent.Contains("Недоступный вариант", StringComparison.Ordinal))
            .ClickAsync(new MouseEventArgs());

        roles.ShouldBeEmpty();
    }
}
