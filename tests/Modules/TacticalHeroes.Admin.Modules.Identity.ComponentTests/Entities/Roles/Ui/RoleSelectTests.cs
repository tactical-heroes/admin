using Microsoft.AspNetCore.Components.Web;

using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Roles.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Entities.Roles.Ui;

public sealed class RoleSelectTests : IdentityCatalogTestContext
{
    [Fact(DisplayName = "SearchAsync should reuse complete catalog when roles are searched")]
    public async Task SearchAsync_Should_ReuseCompleteCatalog_When_RolesAreSearched()
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
        var autocomplete = component.FindComponent<MudAutocomplete<Guid>>();

        var options = await component.InvokeAsync(() => autocomplete.Instance.SearchFunc!("Role 000", Xunit.TestContext.Current.CancellationToken)!);

        options.ShouldBe([Handler.Roles[0].Id]);
        component.FindComponent<MudChip<Guid>>().Markup.ShouldContain(lastRole.Name);
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

        component.FindComponent<MudChip<Guid>>().Markup.ShouldContain("Player (по умолчанию)");
        component.Find("input").GetAttribute("value").ShouldBeNullOrEmpty();
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

        component.WaitForAssertion(() => component.FindComponent<MudChip<Guid>>().Markup.ShouldContain("Moderator"));
        roles.ShouldBe([IdentityCatalogResponses.RoleId]);
        Handler.Requests.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "Render should add and remove chips when roles are selected")]
    public async Task Render_Should_AddAndRemoveChips_When_RolesAreSelected()
    {
        List<Guid> roles = [];
        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.MultiSelection, true)
            .Add(select => select.Values, roles)
            .Add(select => select.ValuesChanged, values => roles = values));
        await component.InvokeAsync(() => component.FindComponent<MudAutocomplete<Guid>>().Instance.OpenMenuAsync());

        await Popovers.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == "Moderator")
            .ClickAsync(new MouseEventArgs());

        roles.ShouldBe([IdentityCatalogResponses.RoleId]);
        component.Find("input").GetAttribute("value").ShouldBeNullOrEmpty();
        var chip = component.FindComponent<MudChip<Guid>>();
        chip.Markup.ShouldContain("Moderator");
        await chip.Find(".mud-chip-close-button").ClickAsync(new MouseEventArgs());
        roles.ShouldBeEmpty();
        component.FindComponents<MudChip<Guid>>().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Render should retain removable missing role when selected role is absent from catalog")]
    public async Task Render_Should_RetainRemovableMissingRole_When_SelectedRoleIsAbsentFromCatalog()
    {
        Guid missingId = Guid.Parse("2994d18b-f957-4a09-a366-ac1606c31018");
        List<Guid> roles = [missingId];
        var component = Render<RoleSelect>(parameters => parameters
            .Add(select => select.MultiSelection, true)
            .Add(select => select.Values, roles)
            .Add(select => select.ValuesChanged, values => roles = values));
        var chip = component.FindComponent<MudChip<Guid>>();
        chip.Instance.Value.ShouldBe(missingId);
        chip.Markup.ShouldContain("Недоступный вариант");
        roles.ShouldBe([missingId]);

        await chip.Find(".mud-chip-close-button").ClickAsync(new MouseEventArgs());

        roles.ShouldBeEmpty();
    }
}
