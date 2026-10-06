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

Run the admin application:

```bash
dotnet run --project src/TacticalHeroes.Admin/TacticalHeroes.Admin.csproj --launch-profile https
```

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

The server host stores ASP.NET Core Data Protection keys in PostgreSQL when
`DataProtection:Enabled` is true. `ConnectionStrings:PostgreSqlConnectionString` is required in
that mode and must come from secrets. Browser/client projects have no database
dependency. Local development keeps the framework defaults unless enabled explicitly.

Admin shares the API databases `tactical_heroes_dev` and `tactical_heroes_prod`
and their existing connection credentials. The `core-platform` managed-PostgreSQL
reconciliation copies each API TLS connection string into Admin's OpenBao secret
under `applications/tactical-heroes-admin/development` and `production`. Sync
these secrets before deploying the admin values. No additional database, user,
disk, or change to the shared `ci-cd` application chart is required.

The EF context owns only `admin.data_protection_keys` and
`admin.__ef_migrations_history`. API schemas and migration history remain separate.
The application discriminator is `TacticalHeroes.Admin`; environment isolation
comes from separate databases. Key XML is sensitive and is not encrypted at rest
by this configuration, so database and backup access must be restricted.

Migrations are generated explicitly, reviewed, and committed:

```bash
dotnet tool restore
dotnet ef migrations add <MigrationName> --project src/TacticalHeroes.Admin \
  --context AdminDataProtectionDbContext --output-dir Infrastructure/DataProtection/Migrations
```

The `TacticalHeroes.Admin.Ef.Migrator` uses `PANiXiDA.Core.Ef.Migrator` and
`host.RunMigrationsAsync<AdminDataProtectionDbContext>()`, matching the API's core
migrator integration. `appsettings.Migrator.json` contains the context's project
path and migration directory, the local `PostgreSqlConnectionString`, and
`GenerateMigrations=true` / `ApplyMigrations=true`, matching the backend API.
The distinct settings filename avoids collisions with the referenced web host's
`appsettings.json` during publishing. The context configures its own history
schema; the core library generates migrations when the model changes and applies
migrations before the Deployment through the shared chart's migration Job.
CI publishes the migrator before the application image; Kargo promotes both to
the same build tag.

For a manual local migration, supply the connection string through
`ConnectionStrings__PostgreSqlConnectionString`, then run:

```bash
dotnet build tools/TacticalHeroes.Admin.Ef.Migrator
cd tools/TacticalHeroes.Admin.Ef.Migrator/bin/Debug/net10.0
dotnet TacticalHeroes.Admin.Ef.Migrator.dll
```

The first switch from container-local keys to the new database key ring/application
discriminator may require signing in and reloading open forms once. Subsequent pod
replacements reuse the saved keys. Do not delete old keys while their cookies or
tokens are still in use.

With Docker running, the host unit tests exercise real PostgreSQL migrations,
preservation of API data and migration history in the shared database, key
persistence across recreated application service providers, and environment
isolation. After deployment, check that an open form and session survive a pod
replacement and that there are no new missing-key errors. Never log key XML or cookies.

### Application composition

The ASP.NET Core host renders one application on the server, serves the
WebAssembly client, and proxies browser API requests through YARP. UI modules
are Razor Class Libraries registered explicitly by the client shell; they are
not separate SPAs or deployments. The shell and each module expose route
contracts with route templates and typed URL builders; components do not own
raw internal route strings. Kiota client code is generated from
`openapi/tactical-heroes.json` into the API project's intermediate output during
the build and is not committed to the repository.

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
