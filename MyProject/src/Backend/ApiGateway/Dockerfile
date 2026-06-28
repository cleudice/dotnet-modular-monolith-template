# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY global.json .
COPY src/Backend/ApiGateway/*.csproj src/Backend/ApiGateway/

RUN dotnet restore src/Backend/ApiGateway/ApiGateway.csproj

# Copy remaining source
COPY src/Backend/ApiGateway/ src/Backend/ApiGateway/

# Publish
RUN dotnet publish src/Backend/ApiGateway/ApiGateway.csproj \
    --configuration Release \
    --no-restore \
    --output /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD curl -f http://localhost:5000/openapi/v1.json || exit 1

USER app
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
