using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

using PANiXiDA.Core.ResultPattern;

using Polly.Timeout;

using TacticalHeroes.Admin.Api.Generated.Models;

namespace TacticalHeroes.Admin.Api.Errors;

public static class ApiResultExtensions
{
    public static async Task<Result<T>> ToApiResultAsync<T>(
        this Task<T?> task,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            T? response = await task;

            return response is null
                ? Result.Failure<T>(
                    Error.Unexpected("API вернул пустой ответ."))
                : Result.Success(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is
            ApiException or
            HttpRequestException or
            TimeoutException or
            TimeoutRejectedException or
            TaskCanceledException)
        {
            return Result.Failure<T>(MapErrors(exception));
        }
    }

    public static async Task<Result> ToApiResultAsync(
        this Task task,
        CancellationToken cancellationToken)
    {
        try
        {
            await task;
            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is
            ApiException or
            HttpRequestException or
            TimeoutException or
            TimeoutRejectedException or
            TaskCanceledException)
        {
            return Result.Failure(MapErrors(exception));
        }
    }

    private static Error[] MapErrors(Exception exception)
    {
        if (exception is HttpValidationProblemDetails validationProblem)
        {
            return GetValidationErrors(validationProblem);
        }

        string message = GetMessage(exception);

        return
        [
            exception switch
            {
                ApiException { ResponseStatusCode: 400 } => Error.Validation(message),
                ApiException { ResponseStatusCode: 401 } => Error.Unauthorized(message),
                ApiException { ResponseStatusCode: 403 } => Error.Forbidden(message),
                ApiException { ResponseStatusCode: 404 } => Error.NotFound(message),
                ApiException { ResponseStatusCode: 409 } => Error.Conflict(message),
                ApiException { ResponseStatusCode: >= 500 } => Error.Unexpected(message),
                HttpRequestException or TimeoutException or TimeoutRejectedException or
                    TaskCanceledException =>
                    Error.Unexpected(message),
                _ => Error.Unexpected(message),
            },
        ];
    }

    private static Error[] GetValidationErrors(
        HttpValidationProblemDetails problem)
    {
        Error[] errors = problem.Errors?.AdditionalData
            .SelectMany(pair => GetMessages(pair.Value)
                .Select(message => CreateValidationError(pair.Key, message)))
            .Distinct()
            .ToArray() ?? [];

        if (errors.Length > 0)
        {
            return errors;
        }

        return
        [
            Error.Validation(
                GetServerMessage(problem.Detail)
                ?? "Проверьте заполненные поля: API отклонил переданные данные."),
        ];
    }

    private static Error CreateValidationError(string field, string message)
    {
        Error error = Error.Validation(message.Trim());

        return string.Equals(field, "general", StringComparison.OrdinalIgnoreCase)
            ? error
            : error.WithField(field);
    }

    private static string GetMessage(Exception exception)
    {
        if (exception is ApiException { ResponseStatusCode: >= 500 })
        {
            return "API временно недоступен. Попробуйте повторить запрос позже.";
        }

        if (exception is ProblemDetails problem
            && GetServerMessage(problem.Detail) is { } serverMessage)
        {
            return serverMessage;
        }

        return exception switch
        {
            ApiException { ResponseStatusCode: 400 } =>
                "Проверьте заполненные поля: API отклонил переданные данные.",
            ApiException { ResponseStatusCode: 401 } =>
                "API требует авторизацию.",
            ApiException { ResponseStatusCode: 403 } =>
                "У текущего пользователя недостаточно прав для этого действия.",
            ApiException { ResponseStatusCode: 404 } =>
                "Запрошенная сущность не найдена.",
            ApiException { ResponseStatusCode: 409 } =>
                "Изменения конфликтуют с текущим состоянием данных.",
            HttpRequestException =>
                "Не удалось подключиться к Tactical Heroes API.",
            TimeoutException or TimeoutRejectedException or TaskCanceledException =>
                "Tactical Heroes API не ответил вовремя.",
            _ =>
                "Не удалось выполнить запрос. Повторите попытку.",
        };
    }

    private static IEnumerable<string> GetMessages(object? value)
    {
        switch (value)
        {
            case string message when !string.IsNullOrWhiteSpace(message):
                yield return message;
                break;
            case UntypedString message when !string.IsNullOrWhiteSpace(message.GetValue()):
                yield return message.GetValue()!;
                break;
            case UntypedArray messages:
                foreach (UntypedNode item in messages.GetValue())
                {
                    foreach (string message in GetMessages(item))
                    {
                        yield return message;
                    }
                }

                break;
            case IEnumerable<string> messages:
                foreach (string message in messages.Where(message => !string.IsNullOrWhiteSpace(message)))
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
