using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

using TacticalHeroes.Admin.Shared.Model;

namespace TacticalHeroes.Admin.Shared.Navigation;

public static class RouteUriBuilder
{
    public static string Build<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TQuery>(
        string path,
        TQuery queryParameters)
        where TQuery : notnull
    {
        List<(string Name, string? Value)> parameters = [];
        AddQueryParameters(parameters, queryParameters);

        return BuildUri(path, parameters);
    }

    public static string BuildPaged<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TFilter>(
        string path,
        TFilter filter,
        int pageNumber,
        int pageSize)
        where TFilter : notnull
    {
        List<(string Name, string? Value)> parameters = [];
        AddQueryParameters(parameters, filter);

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

        return BuildUri(path, parameters);
    }

    private static string BuildUri(
        string path,
        IEnumerable<(string Name, string? Value)> parameters)
    {
        string[] query = [.. parameters
            .Where(static parameter => !string.IsNullOrWhiteSpace(parameter.Value))
            .Select(static parameter =>
                $"{Uri.EscapeDataString(parameter.Name)}=" +
                Uri.EscapeDataString(parameter.Value!))];

        return query.Length == 0
            ? path
            : $"{path}?{string.Join('&', query)}";
    }

    private static void AddQueryParameters<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TQuery>(
        List<(string Name, string? Value)> parameters,
        TQuery queryParameters)
        where TQuery : notnull
    {
        foreach ((string name, PropertyInfo property) in QueryProperties<TQuery>.Items)
        {
            AddParameterValues(parameters, name, property.GetValue(queryParameters));
        }
    }

    private static void AddParameterValues(
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
            Enum enumeration => enumeration.ToSnakeCase(),
            IFormattable formattable => formattable.ToString(
                format: null,
                CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    private static class QueryProperties<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TQuery>
    {
        public static readonly (string Name, PropertyInfo Property)[] Items =
            [.. typeof(TQuery)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(static property =>
                    property.GetMethod is not null
                    && property.GetIndexParameters().Length == 0)
                .OrderBy(static property => property.MetadataToken)
                .Select(static property => (
                    JsonNamingPolicy.CamelCase.ConvertName(property.Name),
                    property))];
    }
}
