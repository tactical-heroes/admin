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

Before the first run, apply the [Admin database migrations](#data-protection-keys)
to the existing local PostgreSQL database. Then run the admin application:

```bash
dotnet run --project src/TacticalHeroes.Admin/TacticalHeroes.Admin.csproj --launch-profile https
```

The local host uses the development API by default. Open
`https://localhost:5173`; the login page stays on this origin while the BFF
proxies authentication to the API. The OIDC proxy route rewrites the API's
absolute `/login` redirect to the local login page and preserves its query.
The API client must allow `https://localhost:5173/oauth/callback` and
`https://localhost:5173/oauth/logout-callback`.

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

Local development uses the existing `tactical-heroes` database on
`localhost:5432` as `postgres`. The host's `appsettings.Development.json` and the
migrator's `appsettings.Migrator.json` include the local development password.
Override the connection string if your local credentials differ.

Apply the Admin migrations before starting the application. The core migrator
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

User create/update forms send the complete `roleIds` selection; an empty array
removes all role assignments. Role forms preserve and edit `isDefault`, which
controls assignment during registration. Both forms edit a permission matrix from
`/api/v1/claims/permissions`, grouped by `moduleCode`/`moduleDisplayName` and resource.
Rows, action columns and whole modules support bulk selection. Only `permission`
claims can be assigned. Catalog failures offer retry and preserve existing assignments.

Menu items require the resource's `read` permission; empty module groups are hidden.
Opening an existing record requires `read`; the save button requires `update`.
Create and delete actions require their corresponding permissions. Effective permissions combine direct
user claims with role claims. Direct user assignments add access; removing one
does not revoke access inherited from a role. The API enforces permissions itself.

Deploy the matching API with `RemoveNonPermissionClaims` before this frontend.
This migration removes all user and role claims except `permission`. Sign in again so the ID token
and Blazor authentication state contain the current permissions.

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

### Proxy trace context

The admin host does not export distributed traces. YARP therefore passes incoming
trace headers through unchanged instead of generating an unsampled parent span.
Requests without trace context let the API start and sample its own trace, so
Grafana's log-to-trace links work for requests made through the admin. An explicit
upstream sampling decision is preserved. If tracing is added to the admin host,
configure ASP.NET Core, HTTP client, and YARP instrumentation together before
enabling the proxy's activity propagation.
