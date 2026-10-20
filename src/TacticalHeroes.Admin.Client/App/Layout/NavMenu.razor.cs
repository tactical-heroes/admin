using System.Security.Claims;

using Microsoft.AspNetCore.Components;

using TacticalHeroes.Admin.Client.App.Composition;
using TacticalHeroes.Admin.Shared.Authorization;

namespace TacticalHeroes.Admin.Client.App.Layout;

public partial class NavMenu(NavigationManager navigation)
{
    private static IEnumerable<AdminNavigationGroup> VisibleGroups(ClaimsPrincipal user) =>
        AdminModules.NavigationGroups
            .Select(group => group with
            {
                Items = group.Items.Where(item => PermissionAccess.HasPermission(user, item.Permission)).ToArray()
            })
            .Where(group => group.Items.Count > 0);

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
