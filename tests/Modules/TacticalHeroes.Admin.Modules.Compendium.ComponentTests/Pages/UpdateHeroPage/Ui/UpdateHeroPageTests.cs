using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using MudBlazor;

using TacticalHeroes.Admin.Shared.Ui.Forms;
using TacticalHeroes.Admin.Shared.Ui.Layout;

using UpdateHeroPageComponent = TacticalHeroes.Admin.Modules.Compendium.Pages.UpdateHeroPage.Ui.UpdateHeroPage;

namespace TacticalHeroes.Admin.Modules.Compendium.ComponentTests.Pages.UpdateHeroPage.Ui;

public sealed class UpdateHeroPageTests : HeroFormTestContext
{
    [Fact(DisplayName = "Submit should preserve faction when current faction is absent from options")]
    public void Submit_Should_PreserveFaction_When_CurrentFactionIsAbsentFromOptions()
    {
        Handler.EmptyFactions = true;
        var component = Render<UpdateHeroPageComponent>(parameters => parameters.Add(page => page.Id, HeroId));
        component.WaitForElement("textarea").Change("Updated description.");

        component.Find(".submit-action").Click();

        component.WaitForAssertion(() =>
        {
            Handler.Saves.ShouldBe(1);
            Handler.SavedHero.GetProperty("factionId").GetGuid().ShouldBe(FactionId);
            Handler.SavedHero.GetProperty("description").GetString().ShouldBe("Updated description.");
            Handler.FactionRequests.ShouldBe([null]);
        });
    }

    [Fact(DisplayName = "Render should load all fields and link to heroes when hero exists")]
    public void Render_Should_LoadAllFieldsAndLinkToHeroes_When_HeroExists()
    {
        var component = Render<UpdateHeroPageComponent>(parameters => parameters.Add(page => page.Id, HeroId));

        component.WaitForAssertion(() =>
        {
            component.FindComponent<PageBackButton>().Find("a").GetAttribute("href").ShouldBe(CompendiumRoutes.Heroes);
            component.FindComponent<EditFormActions>().Find("a").GetAttribute("href").ShouldBe(CompendiumRoutes.Heroes);
            component.FindComponents<MudTextField<string>>()
                .Single(field => field.Instance.Label == "Имя героя").Find("input").GetAttribute("value")
                .ShouldBe("Catherine");
            component.Find("textarea").TextContent.ShouldBe("Leader of the alliance.");
            component.FindComponent<MudAutocomplete<Guid>>().Find("input").GetAttribute("value").ShouldBe("Northern Alliance");
            component.FindComponents<MudNumericField<int>>().Select(field => field.Find("input").GetAttribute("value"))
                .ShouldBe(["12", "8", "3", "7", "4", "2"]);
            component.FindComponent<MudNumericField<double>>().Find("input").GetAttribute("value").ShouldBe("1.5");
        });
    }

    [Fact(DisplayName = "Submit should send edited fields and navigate to heroes when form is valid")]
    public async Task Submit_Should_SendEditedFieldsAndNavigateToHeroes_When_FormIsValid()
    {
        var component = Render<UpdateHeroPageComponent>(parameters => parameters.Add(page => page.Id, HeroId));
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
            Handler.SavedHero.GetProperty("name").GetString().ShouldBe("Catherine");
            Handler.SavedHero.GetProperty("description").GetString().ShouldBe("Updated description.");
            Handler.SavedHero.GetProperty("factionId").GetGuid().ShouldBe(new Guid(1, 0, 0, new byte[8]));
            Handler.SavedHero.GetProperty("attack").GetInt32().ShouldBe(20);
            Handler.SavedHero.GetProperty("defense").GetInt32().ShouldBe(8);
            Handler.SavedHero.GetProperty("minimumDamage").GetInt32().ShouldBe(3);
            Handler.SavedHero.GetProperty("maximumDamage").GetInt32().ShouldBe(7);
            Handler.SavedHero.GetProperty("initiative").GetDouble().ShouldBe(1.5);
            Handler.SavedHero.GetProperty("morale").GetInt32().ShouldBe(4);
            Handler.SavedHero.GetProperty("luck").GetInt32().ShouldBe(2);
            Services.GetRequiredService<NavigationManager>().Uri.ShouldEndWith(CompendiumRoutes.Heroes);
        });
    }

    [Fact(DisplayName = "Submit should reject reversed damage range when maximum is below minimum")]
    public void Submit_Should_RejectReversedDamageRange_When_MaximumIsBelowMinimum()
    {
        var component = Render<UpdateHeroPageComponent>(parameters => parameters.Add(page => page.Id, HeroId));
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
        var component = Render<UpdateHeroPageComponent>(parameters => parameters.Add(page => page.Id, HeroId));

        component.WaitForElement(".submit-action").Click();

        component.WaitForAssertion(() => component.Markup.ShouldContain("Server rejected value."));
    }

    [Fact(DisplayName = "Render should reload the form when failed load is retried")]
    public void Render_Should_ReloadForm_When_FailedLoadIsRetried()
    {
        Handler.FailHeroLoad = true;
        var component = Render<UpdateHeroPageComponent>(parameters => parameters.Add(page => page.Id, HeroId));
        component.WaitForAssertion(() => component.Markup.ShouldContain("Hero unavailable."));
        Handler.FailHeroLoad = false;

        component.FindAll("button").Single(button => button.TextContent.Trim() == "Повторить").Click();

        component.WaitForAssertion(() =>
        {
            Handler.HeroLoads.ShouldBe(2);
            component.Find("textarea").TextContent.ShouldBe("Leader of the alliance.");
            component.Markup.ShouldNotContain("Hero unavailable.");
        });
    }
}
