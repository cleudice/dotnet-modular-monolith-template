Leia AGENTS.md

## Comandos

```bash
cd MyProject

# Build e testes
dotnet build src/Backend/MyProject.slnx
dotnet test src/Backend/MyProject.slnx

# Executar tudo com Docker Compose (recomendado)
docker compose up -d

# Ou executar localmente (2 terminais)
dotnet run --project src/Backend/Host.Api --launch-profile http      # Terminal 1: :5100
dotnet run --project src/Backend/ApiGateway --launch-profile http    # Terminal 2: :5000

# Testar
curl -H "X-User-Id: dev-user" http://localhost:5000/api/catalog/products
```

## Pré-requisitos para execução local

```bash
# PostgreSQL (pule se usar docker compose)
docker run -d --name postgres-dev \
  -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=myproject -p 5432:5432 postgres:17-alpine

# .env (pule se usar docker compose)
cd src/Backend/Host.Api && cp .env.example .env
```

Banco criado automaticamente via `EnsureCreated()` em Development.

## Documentação da API

| Recurso | Dev | Produção |
|---------|-----|----------|
| `http://localhost:5000/openapi/v1.json` | ✅ | ✅ |
| `http://localhost:5000/scalar/v1` (UI) | ✅ | ❌ |

Gateway roteia `/openapi/*` e `/scalar/*` para Host.Api.
Controle em `Program.cs`: `MapOpenApi()` sempre ativo, `MapScalarApiReference()` só em `IsDevelopment()`.

**Padrão de documentação:**
- Endpoints: `.WithSummary()` + `.WithDescription()` no `CatalogEndpoints.cs`
- DTOs/Commands: `[property: Description("...")]` via `System.ComponentModel`
- XML docs habilitados: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
- Spec resultante contém descrições para consumidores externos

## Health checks

```bash
curl http://localhost:5000/health        # Liveness
curl http://localhost:5000/health/ready   # Readiness (DB check)
```

## Observabilidade

- **Serilog** — structured JSON logging (Console). Dev: formato legível, Prod: JSON.
- Config em `appsettings.json` → `Serilog` section.
- Health checks com `AddHealthChecks()` + `AddDbContextCheck<CatalogDbContext>()`.

## Qualidade

- `TreatWarningsAsErrors=true` — zero warnings em Release.
- CA1707 (underscores em nomes de teste) suprimido em `Backend.Tests.csproj`.
- `EnforceCodeStyleInBuild=true` + `AnalysisLevel=latest-recommended`.

## Outbox Pattern

- **Shared**: `OutboxMessage` (entidade), `AppDbContext.CaptureOutboxMessages()`, `OutboxBackgroundService<TContext>` (BackgroundService)
- **Per module**: cada DbContext ganha `DbSet<OutboxMessage>` → tabela isolada no schema do módulo
- **Fluxo**: SaveChanges → serializa domain events → salva na tabela OutboxMessages (mesma transação) → BackgroundService processa via IDomainEventDispatcher
- **Sem duplicação**: lógica de captura no `AppDbContext` base, processamento genérico `OutboxBackgroundService<TContext>`
- **Registro**: `services.AddHostedService<OutboxBackgroundService<CatalogDbContext>>()`

## Autenticação

- **JWT Bearer**: Configurar `Jwt:Key` no `.env` → JWT `sub` claim vira OwnerId.
- **Dev fallback**: Sem `Jwt:Key` configurado, usa header `X-User-Id` (default: `anonymous`).
- **Transição**: `GetOwnerId()` em `CatalogEndpoints.cs` prefere JWT, fallback X-User-Id.

## Testes

```bash
# Unitários (InMemory, rápidos)
dotnet test --filter "FullyQualifiedName~ProductRepositoryTests"

# Integração (PostgreSQL real via Testcontainers — requer Docker)
DOCKER_HOST=unix:///run/user/1000/podman/podman.sock \
  dotnet test --filter "FullyQualifiedName~ProductRepositoryIntegrationTests"
```

## Portas

| Serviço | HTTP | HTTPS |
|---------|------|-------|
| ApiGateway (YARP) | 5000 | 5001 |
| Host.Api (interno) | 5100 | 5101 |
