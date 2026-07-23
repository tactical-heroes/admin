using MudBlazor;

namespace TacticalHeroes.Admin.Client.App.Layout;

internal static class AppTheme
{
    internal static MudTheme Current { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#D94F3D",
            Secondary = "#68758A",
            Background = "#F4F6F9",
            Surface = "#FFFFFF",
            AppbarBackground = "#17202D",
            AppbarText = "#F7F9FC",
            DrawerBackground = "#111923",
            DrawerText = "#DDE5EF",
            TextPrimary = "#1A2230",
            TextSecondary = "#667085",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
        },
    };
}
