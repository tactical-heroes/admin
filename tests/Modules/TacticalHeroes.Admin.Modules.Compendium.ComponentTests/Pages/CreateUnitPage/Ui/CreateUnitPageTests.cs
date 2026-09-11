using System.Text.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;

using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Layout;

using CreateUnitPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.CreateUnitPage.Ui.CreateUnitPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages.CreateUnitPage.Ui;

public sealed class CreateUnitPageTests : UnitFormTestContext
{
    [Fact(DisplayName = "Submit should send explicit null ranged fields when creating a melee unit")]
    public async Task Submit_Should_SendExplicitNullRangedFields_When_CreatingAMeleeUnit()
    {
        var component = Render<CreateUnitPageComponent>();
        await FillFormAsync(component);
        foreach (var field in component.FindComponents<MudNumericField<int?>>())
        {
            field.Find("input").Change(string.Empty);
        }

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            Handler.SavedUnit.GetProperty("shots").ValueKind.ShouldBe(JsonValueKind.Null);
            Handler.SavedUnit.GetProperty("rangedAttackRange").ValueKind.ShouldBe(JsonValueKind.Null);
        });
    }

    [Theory(DisplayName = "Submit should reject partial ranged attack when only one ranged field is provided")]
    [InlineData("Количество выстрелов")]
    [InlineData("Дальность стрельбы")]
    public async Task Submit_Should_RejectPartialRangedAttack_When_OnlyOneRangedFieldIsProvided(string label)
    {
        var component = Render<CreateUnitPageComponent>();
        await FillFormAsync(component);
        component.FindComponents<MudNumericField<int?>>()
            .Single(field => field.Instance.Label == label).Find("input").Change(string.Empty);

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(0);
            component.Markup.ShouldContain("Укажите количество выстрелов и дальность стрельбы вместе");
        });
    }

    [Fact(DisplayName = "Render should link back and cancel to units when form is displayed")]
    public void Render_Should_LinkBackAndCancelToUnits_When_FormIsDisplayed()
    {
        var component = Render<CreateUnitPageComponent>();

        component.FindComponent<PageHeader>().Instance.Title.ShouldBe("Новый юнит");
        component.FindComponent<PageBackButton>().Find("a").GetAttribute("href").ShouldBe(CompendiumRoutes.Units);
        component.FindComponent<EditFormActions>().Find("a").GetAttribute("href").ShouldBe(CompendiumRoutes.Units);
    }

    [Fact(DisplayName = "Submit should send all fields and navigate to unit when form is valid")]
    public async Task Submit_Should_SendAllFieldsAndNavigateToUnit_When_FormIsValid()
    {
        var component = Render<CreateUnitPageComponent>();
        await FillFormAsync(component);

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            Handler.SaveMethod.ShouldBe(HttpMethod.Post);
            Handler.SavedUnit.GetProperty("name").GetString().ShouldBe("Archer");
            Handler.SavedUnit.GetProperty("description").GetString().ShouldBe("Ranged unit of the alliance.");
            Handler.SavedUnit.GetProperty("factionId").GetGuid().ShouldBe(FactionId);
            Handler.SavedUnit.GetProperty("attack").GetInt32().ShouldBe(12);
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
            Services.GetRequiredService<NavigationManager>().Uri.ShouldEndWith(CompendiumRoutes.Unit(UnitId));
        });
    }

    [Fact(DisplayName = "Submit should require name description and faction when form is empty")]
    public void Submit_Should_RequireNameDescriptionAndFaction_When_FormIsEmpty()
    {
        var component = Render<CreateUnitPageComponent>();

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(0);
            component.Markup.ShouldContain("Укажите имя юнита");
            component.Markup.ShouldContain("Укажите описание юнита");
            component.Markup.ShouldContain("Выберите фракцию");
        });
    }

    [Theory(DisplayName = "Submit should display field errors when server rejects value")]
    [InlineData("Name")]
    [InlineData("FactionId")]
    [InlineData("MaximumDamage")]
    [InlineData("Health")]
    [InlineData("Speed")]
    [InlineData("Shots")]
    [InlineData("RangedAttackRange")]
    public async Task Submit_Should_DisplayFieldErrors_When_ServerRejectsValue(string field)
    {
        Handler.RejectSaveField = field;
        var component = Render<CreateUnitPageComponent>();
        await FillFormAsync(component);

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            component.Markup.ShouldContain("Server rejected value.");
            component.Find(".submit-action").HasAttribute("disabled").ShouldBeFalse();
            Services.GetRequiredService<NavigationManager>().Uri.ShouldNotContain(UnitId.ToString());
        });
    }
}
