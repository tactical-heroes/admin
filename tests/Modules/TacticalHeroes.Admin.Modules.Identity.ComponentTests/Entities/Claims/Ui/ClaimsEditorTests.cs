using MudBlazor.Services;

using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Model;
using TacticalHeroes.Admin.Modules.Identity.Entities.Claims.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Entities.Claims.Ui;

public sealed class ClaimsEditorTests : BunitContext
{
    public ClaimsEditorTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact(DisplayName = "Adds a claim and renders its row")]
    public void AddClaim_Should_AddClaimAndRenderRow_When_ClaimsAreEmpty()
    {
        IList<ClaimValue> claims = [];
        var component = Render<ClaimsEditor>(parameters => parameters
            .Add(editor => editor.Claims, claims));

        component.Find("button").Click();

        claims.Count.ShouldBe(1);
        component.FindAll(".claim-row").Count.ShouldBe(1);
        component.FindAll(".empty-claims").ShouldBeEmpty();
    }

    [Fact(DisplayName = "Removes a claim and renders the empty state")]
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
