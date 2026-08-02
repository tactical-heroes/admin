using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TacticalHeroes.Admin.Client.App.Layout;

public partial class AdminHeader
{
    [Parameter]
    public EventCallback<MouseEventArgs> OnMenuClick { get; set; }
}
