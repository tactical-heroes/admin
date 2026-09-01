namespace TacticalHeroes.Admin.Modules.Identity.Pages.RoleListPage.Model;

public sealed class RoleListItem(Guid id, string name)
{
    public Guid Id { get; } = id;

    public string Name { get; } = name;
}
