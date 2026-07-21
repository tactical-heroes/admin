# Tactical Heroes Admin

Административная панель Tactical Heroes на .NET 10 и Blazor Web App.

## Технологии

- Blazor Interactive Auto: первый вход работает через Interactive Server, WebAssembly загружается в фоне и используется на последующих входах;
- MudBlazor для интерфейса;
- YARP как same-origin BFF/API gateway для браузерного клиента;
- Kiota для строготипизированного клиента по OpenAPI.

## Структура

```text
src/
  TacticalHeroes.Admin/         ASP.NET Core host и YARP-BFF
  TacticalHeroes.Admin.Client/  Auto-интерактивный UI
    App/                         композиция, routing и layout
    Pages/                       страницы маршрутов
    Features/                    пользовательские сценарии редактирования
    Entities/                    модели и API-адаптеры Roles/Users
    Shared/                      общие API- и UI-примитивы
  TacticalHeroes.Admin.Api/     Kiota-клиент, генерируемый при сборке в obj
openapi/                         зафиксированный OpenAPI-контракт
```

Структура Client-проекта адаптирует Feature-Sliced Design к Blazor. Зависимости
на транспортные Kiota-модели остаются в `Entities/*/Api`, страницы работают с
UI-моделями.

## Локальный запуск

```powershell
dotnet restore .\TacticalHeroes.Admin.slnx
dotnet run --project .\src\TacticalHeroes.Admin
```

По умолчанию используется
`https://dev.api.tactical-heroes.panixida.ru`. Адрес задаётся параметром
`TacticalHeroesApi:BaseUrl`.

Dev API сейчас требует авторизацию и возвращает `401 Unauthorized` для Roles и
Users. Авторизация сознательно не реализована на этом этапе; интерфейс показывает
понятное состояние ошибки.

## OpenAPI и генерация API-клиента

OpenAPI-контракт хранится в `openapi/tactical-heroes.json`. При сборке проекта
`TacticalHeroes.Admin.Api` MSBuild автоматически восстанавливает локальный Kiota
tool, генерирует ветки Roles/Users в `obj` и подключает их к компиляции. Generated-
исходники не хранятся в Git.

Чтобы обновить сам контракт:

```powershell
Invoke-WebRequest `
  -Uri "https://dev.api.tactical-heroes.panixida.ru/openapi/v1.json" `
  -OutFile ".\openapi\tactical-heroes.json"

dotnet build .\TacticalHeroes.Admin.slnx
```
