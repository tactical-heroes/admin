using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

using TacticalHeroes.Admin.Shared.Model;

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

    public static string BuildPaged<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFilter>(
        string path,
        TFilter filter,
        int pageNumber,
        int pageSize)
        where TFilter : notnull
    {
        var parameters = new List<(string Name, string? Value)>();

        foreach ((string name, PropertyInfo property) in FilterProperties<TFilter>.Items)
        {
            AddParameters(parameters, name, property.GetValue(filter));
        }

        parameters.Add((
            "page",
            pageNumber == 1
                ? null
                : pageNumber.ToString(CultureInfo.InvariantCulture)));
        parameters.Add((
            "pageSize",
            pageSize == PaginationOptions.DefaultPageSize
                ? null
                : pageSize.ToString(CultureInfo.InvariantCulture)));

        return Build(path, [.. parameters]);
    }

    private static void AddParameters(
        List<(string Name, string? Value)> parameters,
        string name,
        object? value)
    {
        if (value is IEnumerable values and not string)
        {
            foreach (object? item in values)
            {
                parameters.Add((name, FormatValue(item)));
            }

            return;
        }

        parameters.Add((name, FormatValue(value)));
    }

    private static string? FormatValue(object? value)
    {
        return value switch
        {
            null => null,
            string text => text,
            IFormattable formattable => formattable.ToString(
                format: null,
                CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    private static class FilterProperties<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFilter>
    {
        public static readonly (string Name, PropertyInfo Property)[] Items =
            typeof(TFilter)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(static property =>
                    property.GetMethod is not null
                    && property.GetIndexParameters().Length == 0)
                .OrderBy(static property => property.MetadataToken)
                .Select(static property => (
                    JsonNamingPolicy.CamelCase.ConvertName(property.Name),
                    property))
                .ToArray();
    }
}
