FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG ENVIRONMENT=""
WORKDIR /app

COPY ["Pl/Ui/Blazor/Pl.Ui.Blazor.Admin/Pl.Ui.Blazor.Admin.csproj", "Pl/Ui/Blazor/Pl.Ui.Blazor.Admin/"]

COPY ["Pl/Ui/Blazor/Pl.Ui.Blazor.ViewModels/Pl.Ui.Blazor.ViewModels.csproj", "Pl/Ui/Blazor/Pl.Ui.Blazor.ViewModels/"]
COPY ["Pl/Ui/Blazor/Pl.Ui.Blazor.Mappers/Pl.Ui.Blazor.Mappers.csproj", "Pl/Ui/Blazor/Pl.Ui.Blazor.Mappers/"]
COPY ["Pl/Ui/Blazor/Pl.Ui.Blazor.Validators/Pl.Ui.Blazor.Validators.csproj", "Pl/Ui/Blazor/Pl.Ui.Blazor.Validators/"]

COPY ["Pl/Ui/Blazor/Services/Pl.Ui.Blazor.Services.DependencyInjection/Pl.Ui.Blazor.Services.DependencyInjection.csproj", "Pl/Ui/Blazor/Services/Pl.Ui.Blazor.Services.DependencyInjection/"]
COPY ["Pl/Ui/Blazor/Services/Pl.Ui.Blazor.Services.Implementations/Pl.Ui.Blazor.Services.Implementations.csproj", "Pl/Ui/Blazor/Services/Pl.Ui.Blazor.Services.Implementations/"]
COPY ["Pl/Ui/Blazor/Services/Pl.Ui.Blazor.Services.Interfaces/Pl.Ui.Blazor.Services.Interfaces.csproj", "Pl/Ui/Blazor/Services/Pl.Ui.Blazor.Services.Interfaces/"]

COPY ["Pl/Api/Http/Pl.Api.Http.Dtos/Pl.Api.Http.Dtos.csproj", "Pl/Api/Http/Pl.Api.Http.Dtos/"]

COPY ["Common/Clients/Common.Clients.DependencyInjection/Common.Clients.DependencyInjection.csproj", "Common/Clients/Common.Clients.DependencyInjection/"]
COPY ["Common/Clients/Common.Clients.Implementations/Common.Clients.Implementations.csproj", "Common/Clients/Common.Clients.Implementations/"]
COPY ["Common/Clients/Common.Clients.Interfaces/Common.Clients.Interfaces.csproj", "Common/Clients/Common.Clients.Interfaces/"]

COPY ["Common/Common.Storage.Dtos/Common.Storage.Dtos.csproj", "Common/Common.Storage.Dtos/"]
COPY ["Common/Common.ConvertParams/Common.ConvertParams.csproj", "Common/Common.ConvertParams/"]
COPY ["Common/Common.SearchParams/Common.SearchParams.csproj", "Common/Common.SearchParams/"]
COPY ["Common/Common.Constants/Common.Constants.csproj", "Common/Common.Constants/"]
COPY ["Common/Common.JsonConverters/Common.JsonConverters.csproj", "Common/Common.JsonConverters/"]
COPY ["Common/Common.Helpers/Common.Helpers.csproj", "Common/Common.Helpers/"]
COPY ["Common/Common.Enums/Common.Enums.csproj", "Common/Common.Enums/"]

RUN dotnet restore "./Pl/Ui/Blazor/Pl.Ui.Blazor.Admin/Pl.Ui.Blazor.Admin.csproj"

COPY . ./
WORKDIR "/app/Pl/Ui/Blazor/Pl.Ui.Blazor.Admin"
RUN dotnet publish "./Pl.Ui.Blazor.Admin.csproj" -c $BUILD_CONFIGURATION -o out -p:WasmApplicationEnvironmentName="$ENVIRONMENT"

FROM nginx:1.23-alpine
WORKDIR /app
EXPOSE 8080
COPY Pl/Ui/Blazor/Pl.Ui.Blazor.Admin/nginx.conf /etc/nginx/nginx.conf
COPY --from=build /app/Pl/Ui/Blazor/Pl.Ui.Blazor.Admin/out/wwwroot /usr/share/nginx/html
