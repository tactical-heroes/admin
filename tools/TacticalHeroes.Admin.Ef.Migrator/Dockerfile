# See https://aka.ms/customizecontainer to learn how to customize your container.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["nuget.config", "."]
COPY ["global.json", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["tools/TacticalHeroes.Admin.Ef.Migrator/TacticalHeroes.Admin.Ef.Migrator.csproj", "tools/TacticalHeroes.Admin.Ef.Migrator/"]
COPY ["src/TacticalHeroes.Admin/TacticalHeroes.Admin.csproj", "src/TacticalHeroes.Admin/"]
COPY ["src/TacticalHeroes.Admin.Api/TacticalHeroes.Admin.Api.csproj", "src/TacticalHeroes.Admin.Api/"]
COPY ["src/TacticalHeroes.Admin.Client/TacticalHeroes.Admin.Client.csproj", "src/TacticalHeroes.Admin.Client/"]
COPY ["src/TacticalHeroes.Admin.Shared/TacticalHeroes.Admin.Shared.csproj", "src/TacticalHeroes.Admin.Shared/"]
COPY ["src/Modules/TacticalHeroes.Admin.Modules.Compendium/TacticalHeroes.Admin.Modules.Compendium.csproj", "src/Modules/TacticalHeroes.Admin.Modules.Compendium/"]
COPY ["src/Modules/TacticalHeroes.Admin.Modules.Identity/TacticalHeroes.Admin.Modules.Identity.csproj", "src/Modules/TacticalHeroes.Admin.Modules.Identity/"]

RUN dotnet restore "./tools/TacticalHeroes.Admin.Ef.Migrator/TacticalHeroes.Admin.Ef.Migrator.csproj"

COPY . .
WORKDIR "/src/tools/TacticalHeroes.Admin.Ef.Migrator"
RUN dotnet build "./TacticalHeroes.Admin.Ef.Migrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TacticalHeroes.Admin.Ef.Migrator.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TacticalHeroes.Admin.Ef.Migrator.dll"]
