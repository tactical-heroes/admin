namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Lists;

public sealed class EntityLinkTests : BunitContext
{
    [Fact(DisplayName = "Render should link the entity name when text and href are provided")]
    public void Render_Should_LinkEntityName_When_TextAndHrefAreProvided()
    {
        IRenderedComponent<EntityLink> component = Render<EntityLink>(parameters => parameters
            .Add(link => link.Href, "/factions/d1703c92-a294-4584-8a9a-78469111363d")
            .Add(link => link.Text, "Elves"));

        var anchor = component.Find("a");
        anchor.GetAttribute("href").ShouldBe("/factions/d1703c92-a294-4584-8a9a-78469111363d");
        anchor.TextContent.Trim().ShouldBe("Elves");
    }
}
