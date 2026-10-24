# Tactical Heroes Admin

Administrative web application for Tactical Heroes.

## Stack

- .NET 10
- ASP.NET Core
- Blazor Web App with Interactive Auto render mode
- MudBlazor
- YARP
- Kiota
- Docker
- Helm
- GitHub Actions

## Local Development

Restore packages:

```bash
dotnet restore TacticalHeroes.Admin.slnx
```

Build the solution:

```bash
dotnet build TacticalHeroes.Admin.slnx --configuration Release
```

Run tests:

```bash
dotnet test TacticalHeroes.Admin.slnx --configuration Release
```

Apply the [Admin migrations](#data-protection-keys), then run the application:

```bash
dotnet run --project src/TacticalHeroes.Admin/TacticalHeroes.Admin.csproj --launch-profile https
```

Local UI and login use `https://localhost:5173` with the dev API. Register
this origin's `/oauth/callback` and `/oauth/logout-callback` in the API client.

## Repository Layout

- `src/TacticalHeroes.Admin/` - ASP.NET Core host and same-origin YARP gateway.
- `src/TacticalHeroes.Admin.Client/` - Interactive Auto application shell, routing, layouts, and module composition.
- `src/Modules/` - module RCL projects: Identity for account and access management, Compendium for game content administration.
- `src/TacticalHeroes.Admin.Api/` - generated Kiota client and shared API transport primitives.
- `src/TacticalHeroes.Admin.Shared/` - reusable presentation primitives without domain dependencies.
- `tests/` - module component, API unit, shared component, and architecture tests.
- `openapi/` - pinned Tactical Heroes API contract.
- `deploy/helm/` - Helm deployment values.

## Initialization Notes

### Data Protection keys

The server stores Data Protection keys in `admin.data_protection_keys` in the
existing API database, with migration history in `admin.__ef_migrations_history`.
Set `ConnectionStrings:PostgreSqlConnectionString` in configuration or override
the local default through `ConnectionStrings__PostgreSqlConnectionString`.
Development and production use separate databases.

Local defaults target `tactical-heroes` on `localhost:5432`; override the
connection string for other credentials.

Local development requires PostgreSQL and the Admin migrations. The core migrator
generates and applies migrations using `appsettings.Migrator.json`:

```bash
dotnet build tools/TacticalHeroes.Admin.Ef.Migrator
cd tools/TacticalHeroes.Admin.Ef.Migrator/bin/Debug/net10.0
dotnet TacticalHeroes.Admin.Ef.Migrator.dll
```

Review and commit generated migrations. Deployment runs the migration Job before
the application. The first switch to database keys may require signing in again;
subsequent pod replacements reuse the saved keys.

### Application composition

The ASP.NET Core host renders one application on the server, serves the
WebAssembly client, and proxies browser API requests through YARP. UI modules
are Razor Class Libraries registered explicitly by the client shell; they are
not separate SPAs or deployments. The shell and each module expose route
contracts with route templates and typed URL builders; components do not own
raw internal route strings. Kiota client code is generated from
`openapi/tactical-heroes.json` into the API project's intermediate output during
the build and is not committed to the repository.

Users support multiple roles and permission matrices. UI visibility follows
permissions; the API enforces access independently. Apply the API's
`RemoveNonPermissionClaims` migration (deletes non-permission claims) before
deploying this frontend, then sign in again.

Successful API responses with `X-Session-Refresh-Required: true` trigger a session
refresh and reload the current page, preserving its query string and fragment.

The client shell and module RCLs follow Feature-Sliced Design. Each `Pages` slice
owns its route, form models, validators, and API adapters. Create and update flows
use separate page slices; reusable domain controls belong in `Entities`.
List filters, page numbers, and sorting are query-string state, so list views can be
bookmarked and restored. Repeated `sort` parameters preserve sort priority, for example
`?sort=name&sort=-id`. Fields use camelCase; a leading `-` means descending and no prefix means ascending.
Legacy `Field:asc` / `Field:desc` links remain supported. API requests still use
`field:asc` / `field:desc`. Click column headings to cycle ascending, descending,
and no sorting; Ctrl/Command + click adds columns in selection order, and Alt + click
removes a criterion. Lists use MudDataGrid with server-side ordering and the shared
page-size selector and numbered pagination. Sorting changes
reset the page while retaining filters and page size. The status display name column
is not sortable until the API supports translating its projection to SQL.
Razor markup, component code, and isolated styles are kept in `.razor`,
`.razor.cs`, and `.razor.css` files respectively. Architecture tests enforce
the allowed FSD folders, dependency direction, module isolation, typed route
state, and Razor file conventions.

### Observability

The server host uses `PANiXiDA.Core.Observability` to export .NET runtime and HTTP
metrics, application logs, and distributed traces over OTLP. This package is not
referenced by the WebAssembly client. Helm values send telemetry to the platform
collector and set separate service names for development and production:
`tactical-heroes-admin-development` and `tactical-heroes-admin-production`.

Core's `AddAspNetCoreInstrumentation()` already subscribes to the built-in .NET 10
Blazor meters; the admin does not need to register them again:

- `Microsoft.AspNetCore.Components`: navigation and event-handler duration.
- `Microsoft.AspNetCore.Components.Lifecycle`: parameter updates and render batches.
- `Microsoft.AspNetCore.Components.Server.Circuits`: active circuits in memory,
  connected circuits, and circuit lifetime. A circuit represents a browser tab,
  not a unique user. These measurements cover server execution, not browser WASM.

YARP participates in tracing alongside ASP.NET Core and HTTP client instrumentation.
Proxied API requests retain the trace ID and upstream sampling decision while
receiving a child span context, so the admin and API spans belong to one trace.

For local collection, set `OTEL_SERVICE_NAME=tactical-heroes-admin-local` and
`OTEL_EXPORTER_OTLP_ENDPOINT` to an accessible collector (the SDK default is
`http://localhost:4317`). No collector credentials belong in source control.

After deployment, select the admin service in Application Telemetry and inspect
runtime memory, GC, JIT, and HTTP metrics. Blazor circuit and component metrics
are exported separately and need matching Grafana queries/panels. Compare a
warmed-up process with connected and disconnected circuits when investigating
memory; process memory growth is not a per-user allocation measurement.

### WebAssembly telemetry through the BFF

The WASM client uses `PANiXiDA.Core.Observability.WebAssembly` 1.1.0. Its
`IHttpClientFactory` API clients create browser spans and forward trace context
to the BFF. Published client metrics and `ILogger` records use the same OTLP
pipeline. The client package does not bring server Npgsql instrumentation into
the WASM runtime.

Browser exports use same-origin POST requests to `/telemetry/v1/logs`,
`/telemetry/v1/metrics`, and `/telemetry/v1/traces`. The BFF accepts authenticated
session cookies, protobuf bodies up to 64 KiB, and at most 120 requests per
minute per user (or per IP for unauthenticated requests). It rejects cross-origin
browser exports, unsupported encodings and other signal paths. Cookie expiry is
still enforced, but telemetry requests do not renew the cookie or refresh OIDC
tokens. Exports before sign-in or after session expiry receive 401 and are dropped.

The BFF forwards only the payload to the internal Collector's frontend receiver
on port 4319; it does not forward browser cookies, Authorization, or client-supplied
metadata headers. The BFF sets `X-Frontend-Service` and `X-Frontend-Environment`.
The Collector overwrites the incoming resource identity from these headers,
keeping browser data separate from server telemetry in the common backends.
`BrowserTelemetry:CollectorEndpoint` configures the internal receiver, and
`BrowserTelemetry:ServiceName` identifies the frontend. Helm sets separate
`tactical-heroes-admin-client-development` and `tactical-heroes-admin-client-production`
service names.

The browser queue is bounded and delivery is best effort. A missing Collector
must not block UI requests; offline data and records pending when a tab closes
can be lost. This integration collects .NET telemetry, not arbitrary JavaScript
console events or Web Vitals.

Release dependencies: merge server observability PR #65, publish the Core
WebAssembly package 1.1.0, and deploy the core-platform frontend receiver before
merging/deploying this client change. For local validation before publication,
pack the Core browser project with `-p:PackageVersion=1.1.0`, restore the admin
against that local feed, then run the normal Release solution build and tests.
