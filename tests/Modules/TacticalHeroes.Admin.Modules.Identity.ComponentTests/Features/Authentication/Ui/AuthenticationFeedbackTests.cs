using MudBlazor;

using TacticalHeroes.Admin.Modules.Identity.Features.Authentication.Ui;

namespace TacticalHeroes.Admin.Modules.Identity.ComponentTests.Features.Authentication.Ui;

public sealed class AuthenticationFeedbackTests : BunitContext
{
    [Fact(DisplayName = "Does not render empty feedback")]
    public void Render_Should_NotRender_When_MessageIsEmpty()
    {
        var component = Render<AuthenticationFeedback>();

        component.Markup.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Renders semantic feedback for its severity")]
    [InlineData(Severity.Error, "alert", "auth-feedback--error")]
    [InlineData(Severity.Warning, "alert", null)]
    [InlineData(Severity.Success, "status", "auth-feedback--success")]
    [InlineData(Severity.Info, "status", null)]
    public void Render_Should_SetSemantics_When_SeverityIsProvided(
        Severity severity,
        string role,
        string? severityClass)
    {
        var component = Render<AuthenticationFeedback>(parameters => parameters
            .Add(feedback => feedback.Message, "Message")
            .Add(feedback => feedback.Severity, severity)
            .Add(feedback => feedback.Class, "additional-class"));

        AngleSharp.Dom.IElement feedback = component.Find("div.auth-feedback");
        feedback.GetAttribute("role").ShouldBe(role);
        feedback.ClassList.ShouldContain("additional-class");

        if (severityClass is not null)
        {
            feedback.ClassList.ShouldContain(severityClass);
        }
    }
}
