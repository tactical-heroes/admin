using PANiXiDA.Core.ResultPattern;

namespace TacticalHeroes.Admin.Api.Errors;

public static class ApiErrorMessage
{
    public static string FromErrors(IReadOnlyList<Error> errors)
    {
        string[] messages = errors
            .Select(error => error.Message.Trim())
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return messages.Length > 0
            ? string.Join(" ", messages)
            : "Не удалось выполнить запрос. Повторите попытку.";
    }

    public static IReadOnlyDictionary<string, string[]> GetFieldErrors(
        IReadOnlyList<Error> errors,
        Func<string, string?>? mapField = null)
    {
        return errors
            .Select(error => (Error: error, Field: GetField(error)))
            .Where(item => item.Field is not null)
            .Select(item => (
                item.Error,
                Field: mapField is null ? item.Field : mapField(item.Field!)))
            .Where(item => !string.IsNullOrWhiteSpace(item.Field))
            .GroupBy(item => item.Field!, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Error.Message)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
    }

    public static IReadOnlyList<Error> GetUnhandledErrors(
        IReadOnlyList<Error> errors,
        Func<string, string?> mapField)
    {
        return errors
            .Where(error => GetField(error) is not { } field || mapField(field) is null)
            .ToArray();
    }

    private static string? GetField(Error error)
    {
        return error.Metadata.TryGetValue(Error.FieldMetadataKey, out object? field)
            ? field as string
            : null;
    }
}
