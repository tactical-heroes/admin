# Tactical Heroes Admin

Административная панель Tactical Heroes на .NET 10 и Blazor Web App.

## Технологии

- Blazor Interactive Auto: первый вход работает через Interactive Server, WebAssembly загружается в фоне и используется на последующих входах;
- MudBlazor для интерфейса;
- YARP как same-origin BFF/API gateway для браузерного клиента;
- OpenID Connect Authorization Code + PKCE + PAR, серверная cookie и автоматическое обновление токенов;
- Kiota для строготипизированного клиента по OpenAPI.

## Структура

```text
src/
  TacticalHeroes.Admin/         ASP.NET Core host и YARP-BFF
  TacticalHeroes.Admin.Client/  Auto-интерактивный UI
    App/                         композиция, routing и layout
    Pages/                       страницы маршрутов
    Features/                    сценарии аутентификации и редактирования
    Entities/                    модели и API-адаптеры Authentication/Roles/Users
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
dotnet run --project .\src\TacticalHeroes.Admin --launch-profile https
```

По умолчанию используется
`https://dev.api.tactical-heroes.panixida.ru`. Адрес задаётся параметром
`TacticalHeroesApi:BaseUrl` и может быть переопределён через environment variable:

```powershell
$env:TacticalHeroesApi__BaseUrl = "https://api.example.com"
$env:Authentication__OpenIdConnect__Authority = "https://api.example.com"
dotnet run --project .\src\TacticalHeroes.Admin
```

Локальный HTTPS-профиль использует `https://localhost:5173`, зарегистрированный
как OAuth callback текущего публичного клиента `tactical-heroes-web`. Токены не
передаются в WebAssembly: SSR использует access token из серверной cookie, а WASM
вызывает защищённый same-origin YARP BFF.

Страница `/login` содержит вход, регистрацию, повторный запрос подтверждения и
запрос восстановления пароля. Страница `/reset-password` принимает `userId` и
`passwordResetToken` из ссылки. Реальная доставка писем в текущем окружении не
проверяется, поэтому регистрация и запросы доступны, но email-сценарий считается
незавершённым до подключения и проверки уведомлений.

## OpenAPI и генерация API-клиента

OpenAPI-контракт хранится в `openapi/tactical-heroes.json`. При сборке проекта
`TacticalHeroes.Admin.Api` MSBuild автоматически восстанавливает локальный Kiota
tool, генерирует ветки Authentication/Roles/Users в `obj` и подключает их к компиляции. Generated-
исходники не хранятся в Git. В `servers` используется относительный URL `/`,
поэтому сгенерированный клиент не привязан к окружению. Server render получает
адрес API из `TacticalHeroesApi:BaseUrl`, а WASM обращается к same-origin YARP BFF.

Чтобы обновить сам контракт:

```powershell
Invoke-WebRequest `
  -Uri "https://dev.api.tactical-heroes.panixida.ru/openapi/v1.json" `
  -OutFile ".\openapi\tactical-heroes.json"

dotnet build .\TacticalHeroes.Admin.slnx
```

## Docker

```powershell
docker build `
  --file .\src\TacticalHeroes.Admin\Dockerfile `
  --tag tactical-heroes-admin:local `
  .

docker run --rm `
  --publish 8080:8080 `
  --env "TacticalHeroesApi__BaseUrl=https://api.example.com" `
  --env "Authentication__OpenIdConnect__Authority=https://api.example.com" `
  --name tactical-heroes-admin `
  tactical-heroes-admin:local
```

Контейнер запускает единый ASP.NET Core host: он отдаёт SSR, обслуживает
Interactive Server, раздаёт WebAssembly и проксирует `/api` через YARP.

Для Kubernetes ingress должен поддерживать WebSocket upgrade. При нескольких
репликах на фазе Interactive Server нужны session affinity и общее хранилище
ASP.NET Core Data Protection keys. В образе включена обработка `X-Forwarded-For`
и `X-Forwarded-Proto` для TLS termination на ingress.

## CI/CD

GitHub Actions проверяет форматирование и Release-сборку. В pull request также
собирается Docker-образ без публикации. После push в `development` или `main`
образ публикуется с тегами `<run_number>` и
`<development|production>-<run_number>`.

Для reusable workflows должны быть настроены:

- variables: `PROJECT_FOLDER=.`, `REGISTRY_URL`, `REGISTRY_IMAGE_PREFIX`,
  `SERVICE_NAME`;
- secrets: `REGISTRY_USER`, `REGISTRY_TOKEN`.

`REGISTRY_TOKEN` должен иметь право публикации образов в выбранный registry.
