namespace TacticalHeroes.Admin.Shared.Navigation;

public static class RouteUriBuilder
{
    public static string Build(
        string path,
        params (string Name, string? Value)[] parameters)
    {
        string[] query = parameters
            .Where(static parameter => !string.IsNullOrWhiteSpace(parameter.Value))
            .Select(static parameter =>
                $"{Uri.EscapeDataString(parameter.Name)}=" +
                Uri.EscapeDataString(parameter.Value!))
            .ToArray();

        return query.Length == 0
            ? path
            : $"{path}?{string.Join('&', query)}";
    }
}
