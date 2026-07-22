using Microsoft.Kiota.Abstractions;

namespace TacticalHeroes.Admin.Client.Shared.Api;

public static class ApiErrorMessage
{
    public static string FromException(Exception exception)
    {
        return exception switch
        {
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
            ApiException { ResponseStatusCode: >= 500 } =>
                "API временно недоступен. Попробуйте повторить запрос позже.",
            HttpRequestException =>
                "Не удалось подключиться к Tactical Heroes API.",
            TaskCanceledException =>
                "Tactical Heroes API не ответил вовремя.",
            _ =>
                "Не удалось выполнить запрос. Повторите попытку.",
        };
    }
}
