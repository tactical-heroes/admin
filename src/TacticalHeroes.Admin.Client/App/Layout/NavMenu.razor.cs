using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Shared.Navigation;

namespace TacticalHeroes.Admin.Client.App.Layout;

public partial class NavMenu(NavigationManager navigation)
{
    private bool IsCurrentGroup(AdminNavigationGroup group)
    {
        string relativePath = navigation.ToBaseRelativePath(navigation.Uri);
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
