namespace TacticalHeroes.Admin.Shared.Navigation;

public sealed record AdminNavigationGroup(
    string Title,
    IReadOnlyList<AdminNavigationItem> Items);

public sealed record AdminNavigationItem(
    string Label,
    string Href,
    string Icon);
