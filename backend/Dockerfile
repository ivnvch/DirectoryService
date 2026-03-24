FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG GITHUB_NUGET_USERNAME
ARG GITHUB_NUGET_OWNER

RUN --mount=type=secret,id=github_token \
    test -n "${GITHUB_NUGET_USERNAME}" && test -n "${GITHUB_NUGET_OWNER}" && \
    dotnet nuget add source https://nuget.pkg.github.com/${GITHUB_NUGET_OWNER}/index.json \
    -n github \
    --username ${GITHUB_NUGET_USERNAME} \
    --password "$(cat /run/secrets/github_token)" \
    --store-password-in-clear-text

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY nuget.config .
COPY ["DirectoryService.API/DirectoryService.API.csproj", "DirectoryService.API/"]
COPY ["DirectoryService.Infrastructure.Postgres/DirectoryService.Infrastructure.Postgres.csproj", "DirectoryService.Infrastructure.Postgres/"]
COPY ["DirectoryService.Application/DirectoryService.Application.csproj", "DirectoryService.Application/"]
COPY ["DirectoryService.Domain/DirectoryService.Domain.csproj", "DirectoryService.Domain/"]
COPY ["DirectoryService.Shared/DirectoryService.Shared.csproj", "DirectoryService.Shared/"]
RUN dotnet restore "DirectoryService.API/DirectoryService.API.csproj"
COPY . .
WORKDIR "/src/DirectoryService.API"
RUN dotnet build "./DirectoryService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.API.dll"]
