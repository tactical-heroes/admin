using MudBlazor;
using MudBlazor.Extensions;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Entities.Claims.Ui;

public sealed class ClaimsEditorTests : IdentityCatalogTestContext
{
    [Fact(DisplayName = "OnInitializedAsync should load permissions when editor is rendered")]
    public void OnInitializedAsync_Should_LoadPermissions_When_EditorIsRendered()
    {
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, []));

        component.WaitForAssertion(() =>
        {
            component.Instance.Permissions!.Items.ShouldHaveSingleItem().ModuleCode.ShouldBe("identity");
            Handler.Requests.ShouldHaveSingleItem().AbsolutePath.ShouldBe("/api/v1/claims/permissions");
            component.Markup.ShouldContain("Управление доступом");
            component.Markup.ShouldContain("Пользователи");
            component.Markup.ShouldNotContain("Показывать в меню");
        });
    }

    [Fact(DisplayName = "SetPermissions should toggle resource when row is selected")]
    public async Task SetPermissions_Should_ToggleResource_When_RowIsSelected()
    {
        IList<ClaimValue> claims =
        [
            new() { Type = "permission", Value = "identity.users.read" },
            new() { Type = "permission", Value = "identity.roles.read" },
            new() { Type = "permission", Value = "compendium.factions.read" }
        ];
        var component = RenderMatrix(claims);
        var row = CheckBox(component, "Пользователи: все действия");
        row.Instance.GetState(box => box.Value).ShouldBeNull();

        await component.InvokeAsync(() => row.Instance.ValueChanged.InvokeAsync(true));
        await component.InvokeAsync(() => CheckBox(component, "Пользователи: все действия")
            .Instance.ValueChanged.InvokeAsync(true));

        claims.Count(claim => claim.Value == "identity.users.read").ShouldBe(1);
        claims.Count(claim => claim.Value == "identity.users.update").ShouldBe(1);
        await component.InvokeAsync(() => CheckBox(component, "Пользователи: все действия")
            .Instance.ValueChanged.InvokeAsync(false));
        claims.Select(claim => claim.Value).ShouldBe(["identity.roles.read", "compendium.factions.read"]);
    }

    [Fact(DisplayName = "ForAction should respect module when column is selected")]
    public async Task ForAction_Should_RespectModule_When_ColumnIsSelected()
    {
        IList<ClaimValue> claims = [new() { Type = "permission", Value = "compendium.factions.read" }];
        var component = RenderMatrix(claims);

        await component.InvokeAsync(() => CheckBox(component, "Управление доступом: Просмотр — все объекты")
            .Instance.ValueChanged.InvokeAsync(true));

        claims.Select(claim => claim.Value).ShouldBe(
            ["compendium.factions.read", "identity.users.read", "identity.roles.read"], ignoreOrder: true);
        CheckBox(component, "Управление доступом: Создание — все объекты").Instance.Disabled.ShouldBeTrue();
        component.FindAll("span[aria-label='Действие не предусмотрено']").ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "IsSelected should preserve other module when tab changes")]
    public async Task IsSelected_Should_PreserveOtherModule_When_TabChanges()
    {
        IList<ClaimValue> claims = [];
        var component = RenderMatrix(claims);
        await component.InvokeAsync(() => CheckBox(component, "Управление доступом: все разрешения")
            .Instance.ValueChanged.InvokeAsync(true));

        await component.InvokeAsync(() => component.FindComponent<MudTabs>().Instance.ActivatePanelAsync(1));
        await component.InvokeAsync(() => CheckBox(component, "Фракции: Просмотр")
            .Instance.ValueChanged.InvokeAsync(true));

        claims.Count.ShouldBe(4);
        claims.ShouldContain(claim => claim.Value == "identity.users.update");
        claims.ShouldContain(claim => claim.Value == "compendium.factions.read");
    }

    [Fact(DisplayName = "LoadAsync should retain claims when catalog fails")]
    public void LoadAsync_Should_RetainClaims_When_CatalogFails()
    {
        Handler.Fail = true;
        IList<ClaimValue> claims = [new() { Type = "permission", Value = "identity.users.read" }];
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, claims));
        component.WaitForElement(".mud-alert");
        claims.ShouldHaveSingleItem().Value.ShouldBe("identity.users.read");
        Handler.Fail = false;

        component.Find("button").Click();

        component.WaitForElement(".permission-table");
        claims.ShouldHaveSingleItem().Value.ShouldBe("identity.users.read");
    }

    [Theory(DisplayName = "IsPermission should normalize known claim types when claims are classified")]
    [InlineData("permission", true)]
    [InlineData(" PERMISSION ", true)]
    [InlineData("custom", false)]
    public void IsPermission_Should_NormalizeKnownClaimTypes_When_ClaimsAreClassified(string type, bool selected)
    {
        var component = RenderMatrix([new ClaimValue { Type = type, Value = "identity.users.read" }]);

        CheckBox(component, "Пользователи: Просмотр").Instance.GetState(box => box.Value).ShouldBe(selected);
    }

    [Theory(DisplayName = "GetSelection should reflect partial selection when row permissions differ")]
    [InlineData(0, false)]
    [InlineData(1, null)]
    [InlineData(2, true)]
    public void GetSelection_Should_ReflectPartialSelection_When_RowPermissionsDiffer(int count, bool? expected)
    {
        IList<ClaimValue> claims = new[] { "identity.users.read", "identity.users.update" }
            .Take(count).Select(value => new ClaimValue { Type = "permission", Value = value }).ToList();

        var component = RenderMatrix(claims);

        CheckBox(component, "Пользователи: все действия").Instance.GetState(box => box.Value).ShouldBe(expected);
    }

    [Fact(DisplayName = "Render should expose only permission matrix when claims belong to catalog")]
    public void Render_Should_ExposeOnlyPermissionMatrix_When_ClaimsBelongToCatalog()
    {
        IList<ClaimValue> claims = [new() { Type = "permission", Value = "identity.users.read" }];

        var component = RenderMatrix(claims);

        component.FindAll(".permission-table").ShouldNotBeEmpty();
        component.FindComponents<MudExpansionPanel>().ShouldBeEmpty();
        component.Markup.ShouldNotContain("Добавить атрибут");
    }

    [Theory(DisplayName = "Validate should reject unsupported claims when type is not permission")]
    [InlineData("section")]
    [InlineData(" SECTION ")]
    [InlineData("custom")]
    public void Validate_Should_RejectUnsupportedClaim_When_TypeIsNotPermission(string type)
    {
        var result = new ClaimValueValidator().Validate(new ClaimValue { Type = type, Value = "identity" });

        result.IsValid.ShouldBeFalse();
    }

    private IRenderedComponent<ClaimsEditor> RenderMatrix(IList<ClaimValue> claims)
    {
        Handler.Permissions = new ClaimCatalog
        {
            Type = "permission",
            Items =
                [
                    Option("identity", "Управление доступом", "users", "Пользователи", "read"),
                    Option("identity", "Управление доступом", "users", "Пользователи", "update"),
                    Option("identity", "Управление доступом", "roles", "Роли", "read"),
                    Option("compendium", "Справочники", "factions", "Фракции", "read")
                ]
        };
        var component = Render<ClaimsEditor>(parameters => parameters.Add(editor => editor.Claims, claims));
        component.WaitForAssertion(() => component.Instance.Permissions.ShouldNotBeNull());
        return component;
    }

    private static ClaimOption Option(string module, string moduleName, string resource, string name, string action) =>
        new()
        {
            Code = $"{module}.{resource}.{action}",
            DisplayName = action,
            ModuleCode = module,
            ModuleDisplayName = moduleName,
            ResourceCode = resource,
            ResourceDisplayName = name
        };

    private static IRenderedComponent<MudCheckBox<bool?>> CheckBox(
        IRenderedComponent<ClaimsEditor> component, string label) =>
        component.FindComponents<MudCheckBox<bool?>>()
            .Single(checkbox => checkbox.Instance.UserAttributes?["aria-label"]?.ToString() == label);
}
