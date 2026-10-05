using System.Text.Json;

namespace TacticalHeroes.Admin.Shared.Navigation;

public static class SortingQuery
{
    public static string Parse(string value)
    {
        value = value.Trim();
        if (value.Contains(':'))
        {
            return value;
        }

        return value.StartsWith('-') ? $"{value[1..]}:desc" : $"{value}:asc";
    }

    public static string Format(string value)
    {
        string[] parts = Parse(value).Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 ||
            (!parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase) &&
             !parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase)))
        {
            return value;
        }

        string field = JsonNamingPolicy.CamelCase.ConvertName(parts[0]);
        return parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? $"-{field}" : field;
    }
}
