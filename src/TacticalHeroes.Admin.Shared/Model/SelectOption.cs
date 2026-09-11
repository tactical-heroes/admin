namespace TacticalHeroes.Admin.Shared.Model;

public class SelectOption<TId> where TId : notnull
{
    public TId Id { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
}
