using System.Reflection;

using PANiXiDA.Core.ResultPattern;

namespace TacticalHeroes.Admin.Shared.Errors;

public static class ApiErrorMessage
{
    private static class ModelFields<TModel>
    {
        public static IReadOnlyDictionary<string, string> Names { get; } =
            typeof(TModel)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.SetMethod?.IsPublic == true)
                .ToDictionary(
                    property => property.Name,
                    property => property.Name,
                    StringComparer.OrdinalIgnoreCase);
    }

    public static string FromErrors(IReadOnlyList<Error> errors)
    {
        string[] messages =
        [
            .. errors
                .Select(error => error.Message.Trim())
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct(StringComparer.Ordinal),
        ];

        return messages.Length > 0
            ? string.Join(" ", messages)
            : "Не удалось выполнить запрос. Повторите попытку.";
    }

    public static IReadOnlyDictionary<string, string[]> GetFieldErrors<TModel>(
        IReadOnlyList<Error> errors)
    {
        return errors
            .Select(error => (Error: error, Field: GetField(error)))
            .Where(item => item.Field is not null)
            .Select(item => (item.Error, Field: MapModelField<TModel>(item.Field!)))
            .Where(item => !string.IsNullOrWhiteSpace(item.Field))
            .GroupBy(item => item.Field!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Error.Message)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<Error> GetUnhandledErrors<TModel>(
        IReadOnlyList<Error> errors)
    {
        return
        [
            .. errors.Where(
                error => GetField(error) is not { } field ||
                    MapModelField<TModel>(field) is null),
        ];
    }

    private static string? GetField(Error error)
    {
        return error.Metadata.TryGetValue(Error.FieldMetadataKey, out object? field)
            ? field as string
            : null;
    }

    private static string? MapModelField<TModel>(string field)
    {
        return ModelFields<TModel>.Names.TryGetValue(field, out string? modelField)
            ? modelField
            : null;
    }
}
