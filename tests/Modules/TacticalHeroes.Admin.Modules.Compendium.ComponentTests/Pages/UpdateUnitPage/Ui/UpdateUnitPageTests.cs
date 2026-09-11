using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;

using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Layout;

using UpdateUnitPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateUnitPage.Ui.UpdateUnitPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages.UpdateUnitPage.Ui;

public sealed class UpdateUnitPageTests : UnitFormTestContext
{
    [Theory(DisplayName = "Submit should omit ranged fields when saving a melee unit")]
    [InlineData(false)]
    [InlineData(true)]
    public void Submit_Should_OmitRangedFields_When_SavingAMeleeUnit(bool wasRanged)
    {
        Handler.Ranged = wasRanged;
        var component = Render<UpdateUnitPageComponent>(parameters => parameters.Add(page => page.Id, UnitId));
        component.WaitForElement("textarea");
        foreach (var field in component.FindComponents<MudNumericField<int?>>())
        {
            field.Find("input").Change(string.Empty);
        }

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            Handler.SaveMethod.ShouldBe(HttpMethod.Put);
            Handler.SavedUnit.TryGetProperty("shots", out _).ShouldBeFalse();
            Handler.SavedUnit.TryGetProperty("rangedAttackRange", out _).ShouldBeFalse();
        });
    }

    [Fact(DisplayName = "Submit should preserve faction when current faction is absent from options")]
    public void Submit_Should_PreserveFaction_When_CurrentFactionIsAbsentFromOptions()
    {
        Handler.EmptyFactions = true;
        var component = Render<UpdateUnitPageComponent>(parameters => parameters.Add(page => page.Id, UnitId));
        component.WaitForElement("textarea").Change("Updated description.");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            Handler.SavedUnit.GetProperty("factionId").GetGuid().ShouldBe(FactionId);
            Handler.SavedUnit.GetProperty("description").GetString().ShouldBe("Updated description.");
            Handler.FactionRequests.ShouldBe([null]);
        });
    }

    [Fact(DisplayName = "Render should load all fields and link to units when unit exists")]
    public void Render_Should_LoadAllFieldsAndLinkToUnits_When_UnitExists()
    {
        var component = Render<UpdateUnitPageComponent>(parameters => parameters.Add(page => page.Id, UnitId));

        component.WaitForAssertion(() =>
        {
            component.FindComponent<PageBackButton>().Find("a").GetAttribute("href").ShouldBe(CompendiumRoutes.Units);
            component.FindComponent<EditFormActions>().Find("a").GetAttribute("href").ShouldBe(CompendiumRoutes.Units);
            component.FindComponents<MudTextField<string>>()
                .Single(field => field.Instance.Label == "Имя юнита").Find("input").GetAttribute("value")
                .ShouldBe("Archer");
            component.Find("textarea").TextContent.ShouldBe("Ranged unit of the alliance.");
            component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
            component.FindComponents<MudNumericField<int>>().Select(field => field.Find("input").GetAttribute("value"))
                .ShouldBe(["12", "8", "3", "7", "4", "2", "25", "6"]);
            component.FindComponents<MudNumericField<int?>>().Select(field => field.Find("input").GetAttribute("value"))
                .ShouldBe(["12", "8"]);
            component.FindComponent<MudNumericField<double>>().Find("input").GetAttribute("value").ShouldBe("1.5");
        });
    }

    [Fact(DisplayName = "Submit should send edited fields and navigate to units when form is valid")]
    public async Task Submit_Should_SendEditedFieldsAndNavigateToUnits_When_FormIsValid()
    {
        var component = Render<UpdateUnitPageComponent>(parameters => parameters.Add(page => page.Id, UnitId));
        component.WaitForElement("textarea").Change("Updated description.");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Атака").Find("input").Change("20");
        component.FindComponent<MudAutocomplete<Guid>>().Find("input").Input("Faction 1");
        await Popovers.WaitForAssertionAsync(() => Popovers.Markup.ShouldContain("Faction 1"));
        await Popovers.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == "Faction 1").ClickAsync(new MouseEventArgs());

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            Handler.SaveMethod.ShouldBe(HttpMethod.Put);
            Handler.SavedUnit.GetProperty("name").GetString().ShouldBe("Archer");
            Handler.SavedUnit.GetProperty("description").GetString().ShouldBe("Updated description.");
            Handler.SavedUnit.GetProperty("factionId").GetGuid().ShouldBe(new Guid(1, 0, 0, new byte[8]));
            Handler.SavedUnit.GetProperty("attack").GetInt32().ShouldBe(20);
            Handler.SavedUnit.GetProperty("defense").GetInt32().ShouldBe(8);
            Handler.SavedUnit.GetProperty("health").GetInt32().ShouldBe(25);
            Handler.SavedUnit.GetProperty("speed").GetInt32().ShouldBe(6);
            Handler.SavedUnit.GetProperty("shots").GetInt32().ShouldBe(12);
            Handler.SavedUnit.GetProperty("rangedAttackRange").GetInt32().ShouldBe(8);
            Handler.SavedUnit.GetProperty("minimumDamage").GetInt32().ShouldBe(3);
            Handler.SavedUnit.GetProperty("maximumDamage").GetInt32().ShouldBe(7);
            Handler.SavedUnit.GetProperty("initiative").GetDouble().ShouldBe(1.5);
            Handler.SavedUnit.GetProperty("morale").GetInt32().ShouldBe(4);
            Handler.SavedUnit.GetProperty("luck").GetInt32().ShouldBe(2);
            Services.GetRequiredService<NavigationManager>().Uri.ShouldEndWith(CompendiumRoutes.Units);
        });
    }

    [Fact(DisplayName = "Submit should reject reversed damage range when maximum is below minimum")]
    public void Submit_Should_RejectReversedDamageRange_When_MaximumIsBelowMinimum()
    {
        var component = Render<UpdateUnitPageComponent>(parameters => parameters.Add(page => page.Id, UnitId));
        component.WaitForElement("textarea");
        component.FindComponents<MudNumericField<int>>()
            .Single(field => field.Instance.Label == "Максимальный урон").Find("input").Change("2");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(0);
            component.Markup.ShouldContain("Максимальный урон не может быть меньше минимального");
        });
    }

    [Fact(DisplayName = "Submit should display field error when server rejects update")]
    public void Submit_Should_DisplayFieldError_When_ServerRejectsUpdate()
    {
        Handler.RejectSaveField = "FactionId";
        var component = Render<UpdateUnitPageComponent>(parameters => parameters.Add(page => page.Id, UnitId));

        component.WaitForElement(".submit-action").Click();

        component.WaitForAssertion(() => component.Markup.ShouldContain("Server rejected value."));
    }

    [Fact(DisplayName = "Render should reload the form when failed load is retried")]
    public void Render_Should_ReloadForm_When_FailedLoadIsRetried()
    {
        Handler.FailUnitLoad = true;
        var component = Render<UpdateUnitPageComponent>(parameters => parameters.Add(page => page.Id, UnitId));
        component.WaitForAssertion(() => component.Markup.ShouldContain("Unit unavailable."));
        Handler.FailUnitLoad = false;

        component.FindAll("button").Single(button => button.TextContent.Trim() == "Повторить").Click();

        component.WaitForAssertion(() =>
        {
            Handler.UnitLoads.ShouldBe(2);
            component.Find("textarea").TextContent.ShouldBe("Ranged unit of the alliance.");
            component.Markup.ShouldNotContain("Unit unavailable.");
        });
    }
}
