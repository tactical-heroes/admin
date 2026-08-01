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
- `src/TacticalHeroes.Admin.Client/` - Interactive Auto UI and generated Kiota client.
- `tests/` - component tests.
- `openapi/` - pinned Tactical Heroes API contract.
- `deploy/helm/` - Helm deployment values.

## Initialization Notes

The ASP.NET Core host renders the application on the server, serves the
WebAssembly client, and proxies browser API requests through YARP. Kiota client
code is generated from `openapi/tactical-heroes.json` into the client project's
intermediate output during the build and is not committed to the repository.
