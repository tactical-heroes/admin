using Microsoft.AspNetCore.Components;

namespace TacticalHeroes.Admin.Client.App.Layout;

public partial class AdminSidebar
{
    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }
}
