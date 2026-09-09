using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Layout;

using CreateHeroPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.CreateHeroPage.Ui.CreateHeroPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages.CreateHeroPage.Ui;

public sealed class CreateHeroPageTests : HeroFormTestContext
{
    [Fact(DisplayName = "Render should link back and cancel to heroes when form is displayed")]
    public void Render_Should_LinkBackAndCancelToHeroes_When_FormIsDisplayed()
    {
        var component = Render<CreateHeroPageComponent>();

        component.FindComponent<PageBackButton>().Find("a").GetAttribute("href").ShouldBe(CompendiumRoutes.Heroes);
        component.FindComponent<EditFormActions>().Find("a").GetAttribute("href").ShouldBe(CompendiumRoutes.Heroes);
    }

    [Fact(DisplayName = "Submit should send all fields and navigate to hero when form is valid")]
    public void Submit_Should_SendAllFieldsAndNavigateToHero_When_FormIsValid()
    {
        var component = Render<CreateHeroPageComponent>();
        FillForm(component);

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            Handler.SaveMethod.ShouldBe(HttpMethod.Post);
            Handler.SavedHero.GetProperty("name").GetString().ShouldBe("Catherine");
            Handler.SavedHero.GetProperty("description").GetString().ShouldBe("Leader of the alliance.");
            Handler.SavedHero.GetProperty("factionId").GetGuid().ShouldBe(FactionId);
            Handler.SavedHero.GetProperty("attack").GetInt32().ShouldBe(12);
            Handler.SavedHero.GetProperty("defense").GetInt32().ShouldBe(8);
            Handler.SavedHero.GetProperty("minimumDamage").GetInt32().ShouldBe(3);
            Handler.SavedHero.GetProperty("maximumDamage").GetInt32().ShouldBe(7);
            Handler.SavedHero.GetProperty("initiative").GetDouble().ShouldBe(1.5);
            Handler.SavedHero.GetProperty("morale").GetInt32().ShouldBe(4);
            Handler.SavedHero.GetProperty("luck").GetInt32().ShouldBe(2);
            Services.GetRequiredService<NavigationManager>().Uri.ShouldEndWith(CompendiumRoutes.Hero(HeroId));
        });
    }

    [Fact(DisplayName = "Submit should require name description and faction when form is empty")]
    public void Submit_Should_RequireNameDescriptionAndFaction_When_FormIsEmpty()
    {
        var component = Render<CreateHeroPageComponent>();

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(0);
            component.Markup.ShouldContain("Укажите имя героя");
            component.Markup.ShouldContain("Укажите описание героя");
            component.Markup.ShouldContain("Выберите фракцию");
        });
    }

    [Theory(DisplayName = "Submit should display field errors when server rejects value")]
    [InlineData("Name")]
    [InlineData("FactionId")]
    [InlineData("MaximumDamage")]
    public void Submit_Should_DisplayFieldErrors_When_ServerRejectsValue(string field)
    {
        Handler.RejectSaveField = field;
        var component = Render<CreateHeroPageComponent>();
        FillForm(component);

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            component.Markup.ShouldContain("Server rejected value.");
            component.Find(".submit-action").HasAttribute("disabled").ShouldBeFalse();
            Services.GetRequiredService<NavigationManager>().Uri.ShouldNotContain(HeroId.ToString());
        });
    }
}
