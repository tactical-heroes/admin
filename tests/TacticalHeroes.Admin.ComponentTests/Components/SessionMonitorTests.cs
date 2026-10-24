using TacticalHeroes.Admin.Components;

namespace TacticalHeroes.Admin.ComponentTests.Components;

public sealed class SessionMonitorTests : BunitContext
{
    [Theory(DisplayName = "Render should monitor session only when user is authenticated")]
    [InlineData(true)]
    [InlineData(false)]
    public void Render_Should_MonitorSessionOnly_When_UserIsAuthenticated(bool authenticated)
    {
        var authorization = this.AddAuthorization();
        if (authenticated)
        {
            authorization.SetAuthorized("Reader");
        }

        var component = Render<SessionMonitor>();

        component.FindAll("script[data-session-url]").Count.ShouldBe(authenticated ? 1 : 0);
        if (authenticated)
        {
            var monitor = component.Find("script[data-session-url]");
            monitor.GetAttribute("data-session-url").ShouldBe("/authentication/session");
            monitor.GetAttribute("data-refresh-url").ShouldBe("/authentication/refresh");
            monitor.GetAttribute("src").ShouldBe("Components/SessionMonitor.razor.js");
            monitor.GetAttribute("data-session-version")!.Length.ShouldBe(64);
        }
    }
}
