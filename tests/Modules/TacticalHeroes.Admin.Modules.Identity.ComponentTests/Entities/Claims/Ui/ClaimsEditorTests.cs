using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Entities.Claims.Ui;

public sealed class ClaimsEditorTests : IdentityCatalogTestContext
{
    [Fact(DisplayName = "OnInitializedAsync should load catalogs when editor is rendered")]
    public void OnInitializedAsync_Should_LoadCatalogs_When_EditorIsRendered()
    {
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, []));

        component.WaitForAssertion(() =>
        {
            component.Instance.Permissions!.Items.ShouldHaveSingleItem().ResourceDisplayName.ShouldBe("Пользователи");
            component.Instance.Sections!.Items.ShouldHaveSingleItem().DisplayName.ShouldBe("Управление доступом");
            Handler.Requests.Count.ShouldBe(2);
        });
    }

    [Theory(DisplayName = "AddClaim should use selected catalog type when add action is clicked")]
    [InlineData("Разрешение", "permission")]
    [InlineData("Раздел", "section")]
    [InlineData("Другой атрибут", "")]
    public void AddClaim_Should_UseSelectedCatalogType_When_AddActionIsClicked(string buttonText, string type)
    {
        IList<ClaimValue> claims = [];
        var component = Render<ClaimsEditor>(parameters => parameters
            .Add(editor => editor.Claims, claims));

        component.FindAll("button").Single(button => button.TextContent.Trim() == buttonText).Click();

        claims.ShouldHaveSingleItem().Type.ShouldBe(type);
        component.FindAll(".claim-row").Count.ShouldBe(1);
        component.FindAll(".empty-claims").ShouldBeEmpty();
    }

    [Fact(DisplayName = "LoadAsync should retain claims and retry when catalogs fail")]
    public void LoadAsync_Should_RetainClaimsAndRetry_When_CatalogsFail()
    {
        Handler.Fail = true;
        IList<ClaimValue> claims = [new() { Type = "permission", Value = "identity.users.read" }];
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, claims));
        component.WaitForElement(".mud-alert");
        claims.ShouldHaveSingleItem().Value.ShouldBe("identity.users.read");
        Handler.Fail = false;

        component.Find("button").Click();

        component.WaitForAssertion(() => component.FindAll(".claim-row").Count.ShouldBe(1));
        claims.ShouldHaveSingleItem().Value.ShouldBe("identity.users.read");
    }

    [Theory(DisplayName = "GetCatalog should select known values when claim type matches catalog")]
    [InlineData("permission", "identity.users.read")]
    [InlineData(" PERMISSION ", "identity.users.read")]
    [InlineData("section", "identity")]
    public async Task GetCatalog_Should_SelectKnownValues_When_ClaimTypeMatchesCatalog(string type, string value)
    {
        IList<ClaimValue> claims = [new() { Type = type }];
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, claims));
        var select = component.FindComponent<MudSelect<string>>();

        await component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(value));

        claims.ShouldHaveSingleItem().Value.ShouldBe(value);
    }

    [Fact(DisplayName = "GetCatalog should preserve unknown values when catalog changes")]
    public void GetCatalog_Should_PreserveUnknownValues_When_CatalogChanges()
    {
        IList<ClaimValue> claims = [new() { Type = "permission", Value = "retired.permission" }];
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, claims));

        component.FindComponents<MudSelectItem<string>>()
            .Single(item => item.Instance.Value == "retired.permission").Instance.Disabled.ShouldBeTrue();
        claims.ShouldHaveSingleItem().Value.ShouldBe("retired.permission");
    }

    [Theory(DisplayName = "GetClaimName should display catalog label when value is selected")]
    [InlineData("identity.users.read", "Пользователи: Просмотр пользователей")]
    [InlineData("retired.permission", "retired.permission (нет в каталоге)")]
    [InlineData("", "")]
    public void GetClaimName_Should_DisplayCatalogLabel_When_ValueIsSelected(string value, string label)
    {
        IList<ClaimValue> claims = [new() { Type = "permission", Value = value }];
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, claims));

        component.FindComponent<MudSelect<string>>().Find("input").GetAttribute("value").ShouldBe(label);
    }

    [Fact(DisplayName = "ChangeType should clear previous value when claim type changes")]
    public void ChangeType_Should_ClearPreviousValue_When_ClaimTypeChanges()
    {
        IList<ClaimValue> claims = [new() { Type = "custom", Value = "old-value" }];
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, claims));

        component.Find(".claim-row input").Change("section");

        claims.ShouldHaveSingleItem().Type.ShouldBe("section");
        claims[0].Value.ShouldBeEmpty();
        component.FindComponent<MudSelect<string>>().ShouldNotBeNull();
    }

    [Fact(DisplayName = "RemoveClaim should remove claim and render empty state when claim exists")]
    public void RemoveClaim_Should_RemoveClaimAndRenderEmptyState_When_ClaimExists()
    {
        IList<ClaimValue> claims =
        [
            new()
            {
                Type = "role",
                Value = "admin"
            }
        ];
        var component = Render<ClaimsEditor>(parameters => parameters
            .Add(editor => editor.Claims, claims));

        component.Find(".claim-remove button").Click();

        claims.ShouldBeEmpty();
        component.FindAll(".claim-row").ShouldBeEmpty();
        component.Find(".empty-claims").ShouldNotBeNull();
    }
}
