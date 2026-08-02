using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Api.Errors;

public static class ApiErrorMessage
{
    public static string FromException(Exception exception)
    {
        return exception switch
        {
            ApiException { ResponseStatusCode: >= 500 } =>
                "API временно недоступен. Попробуйте повторить запрос позже.",
            HttpValidationProblemDetails validationProblem
                when GetValidationMessage(validationProblem) is { } message =>
                message,
            ProblemDetails problem
                when GetServerMessage(problem.Detail) is { } message =>
                message,
            ApiException { ResponseStatusCode: 401 } =>
                "API требует авторизацию. Интерфейс готов, но доступ к данным будет закрыт до подключения входа.",
            ApiException { ResponseStatusCode: 403 } =>
                "У текущего пользователя недостаточно прав для этого действия.",
            ApiException { ResponseStatusCode: 400 } =>
                "Проверьте заполненные поля: API отклонил переданные данные.",
            ApiException { ResponseStatusCode: 404 } =>
                "Запрошенная сущность не найдена.",
            ApiException { ResponseStatusCode: 409 } =>
                "Изменения конфликтуют с текущим состоянием данных.",
            HttpRequestException =>
                "Не удалось подключиться к Tactical Heroes API.",
            TaskCanceledException =>
                "Tactical Heroes API не ответил вовремя.",
            _ =>
                "Не удалось выполнить запрос. Повторите попытку.",
        };
    }

    private static string? GetValidationMessage(
        HttpValidationProblemDetails problem)
    {
        var messages = problem.Errors?.AdditionalData.Values
            .SelectMany(GetMessages)
            .Select(message => message.Trim())
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct(StringComparer.Ordinal)
            .ToArray() ?? [];

        return messages.Length > 0
            ? string.Join(" ", messages)
            : GetServerMessage(problem.Detail);
    }

    private static IEnumerable<string> GetMessages(object? value)
    {
        switch (value)
        {
            case string message:
                yield return message;
                break;
            case UntypedString message:
                var text = message.GetValue();
                if (text is not null)
                {
                    yield return text;
                }

                break;
            case UntypedArray messages:
                foreach (var item in messages.GetValue())
                {
                    foreach (var message in GetMessages(item))
                    {
                        yield return message;
                    }
                }

                break;
            case IEnumerable<string> messages:
                foreach (var message in messages)
                {
                    yield return message;
                }

                break;
        }
    }

    private static string? GetServerMessage(string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? null
            : message.Trim();
    }
}
