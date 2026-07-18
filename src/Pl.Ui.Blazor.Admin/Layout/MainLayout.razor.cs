namespace Pl.Ui.Blazor.Admin.Layout;

public partial class MainLayout
{
    private string _companyName = "Название компании";

    private bool _drawerOpen = true;

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }
}
