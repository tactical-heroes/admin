using MudBlazor;

using TacticalHeroes.Admin.Client.App.Layout;

namespace TacticalHeroes.Admin.Client.ComponentTests.App.Layout;

public sealed class AdminSidebarTests : ClientComponentTestContext
{
    [Fact(DisplayName = "The sidebar renders navigation and forwards drawer state changes")]
    public async Task Render_Should_ForwardDrawerState_When_DrawerCloses()
    {
        bool open = true;
        var component = Render<AdminSidebar>(parameters => parameters
            .Add(sidebar => sidebar.Open, true)
            .Add(sidebar => sidebar.OpenChanged, value => open = value));

        await component.InvokeAsync(() => component.FindComponent<MudDrawer>().Instance.OpenChanged.InvokeAsync(false));

        open.ShouldBeFalse();
        component.FindComponent<NavMenu>().ShouldNotBeNull();
        component.Find("img").GetAttribute("alt").ShouldNotBeNullOrWhiteSpace();
    }
}

