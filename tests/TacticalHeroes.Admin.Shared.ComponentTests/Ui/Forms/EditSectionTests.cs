namespace TacticalHeroes.Admin.Shared.ComponentTests.Ui.Forms;

public sealed class EditSectionTests : BunitContext
{
    [Fact(DisplayName = "Render should show title and content when parameters are provided")]
    public void Render_Should_ShowTitleAndContent_When_ParametersAreProvided()
    {
        var component = Render<EditSection>(parameters => parameters
            .Add(section => section.Title, "Details")
            .AddChildContent("<input aria-label='Name' />"));

        component.Markup.ShouldContain("Details");
        component.Find("input[aria-label='Name']").ShouldNotBeNull();
    }
}
