using TacticalHeroes.Admin.Shared.Ui;

namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui;

public sealed class EntityRowActionsTests : BunitContext
{
    [Fact(DisplayName = "Renders arbitrary row actions in the supplied order")]
    public void ChildContent_Should_RenderAllActions_When_Composed()
    {
        IRenderedComponent<EntityRowActions> component = Render<EntityRowActions>(parameters =>
            parameters.AddChildContent(
                "<button type=\"button\">First</button>" +
                "<a href=\"/items/1\">Second</a>"));

        component.Find("button").TextContent.ShouldBe("First");
        component.Find("a").TextContent.ShouldBe("Second");
    }
}
