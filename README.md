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
- `src/Modules/` - flat set of module RCL projects; Identity contains the current UI and Compendium is connected for future catalog screens.
- `src/TacticalHeroes.Admin.Api/` - generated Kiota client and shared API transport primitives.
- `src/TacticalHeroes.Admin.Shared/` - reusable presentation primitives without domain dependencies.
- `tests/` - module component, API unit, shared component, and architecture tests.
- `openapi/` - pinned Tactical Heroes API contract.
- `deploy/helm/` - Helm deployment values.

## Initialization Notes

The ASP.NET Core host renders one application on the server, serves the
WebAssembly client, and proxies browser API requests through YARP. UI modules
are Razor Class Libraries registered explicitly by the client shell; they are
not separate SPAs or deployments. Kiota client code is generated from
`openapi/tactical-heroes.json` into the API project's intermediate output during
the build and is not committed to the repository.
