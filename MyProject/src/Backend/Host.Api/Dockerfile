# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY global.json .

COPY src/Backend/BuildingBlocks/BuildingBlocks.Domain/*.csproj src/Backend/BuildingBlocks/BuildingBlocks.Domain/
COPY src/Backend/BuildingBlocks/BuildingBlocks.Application/*.csproj src/Backend/BuildingBlocks/BuildingBlocks.Application/
COPY src/Backend/BuildingBlocks/BuildingBlocks.Infrastructure/*.csproj src/Backend/BuildingBlocks/BuildingBlocks.Infrastructure/
COPY src/Backend/Modules/Catalog/Catalog.Domain/*.csproj src/Backend/Modules/Catalog/Catalog.Domain/
COPY src/Backend/Modules/Catalog/Catalog.Application/*.csproj src/Backend/Modules/Catalog/Catalog.Application/
COPY src/Backend/Modules/Catalog/Catalog.Infrastructure/*.csproj src/Backend/Modules/Catalog/Catalog.Infrastructure/
COPY src/Backend/Host.Api/*.csproj src/Backend/Host.Api/

RUN dotnet restore src/Backend/Host.Api/Host.Api.csproj

# Copy remaining source
COPY src/Backend/ src/Backend/

# Publish
RUN dotnet publish src/Backend/Host.Api/Host.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5100

EXPOSE 5100
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD curl -f http://localhost:5100/openapi/v1.json || exit 1

USER app
ENTRYPOINT ["dotnet", "Host.Api.dll"]
