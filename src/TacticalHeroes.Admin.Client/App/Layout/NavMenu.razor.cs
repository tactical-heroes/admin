using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Client.App.Layout;

public partial class NavMenu
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private bool IsCurrentGroup(AdminNavigationGroup group)
    {
        string relativePath = Navigation.ToBaseRelativePath(Navigation.Uri);
        int suffixStart = relativePath.IndexOfAny(['?', '#']);

        if (suffixStart >= 0)
        {
            relativePath = relativePath[..suffixStart];
        }

        string currentPath = $"/{relativePath.Trim('/')}";

        return group.Items.Any(item =>
            currentPath.Equals(item.Href, StringComparison.OrdinalIgnoreCase) ||
            currentPath.StartsWith($"{item.Href}/", StringComparison.OrdinalIgnoreCase));
    }
}
