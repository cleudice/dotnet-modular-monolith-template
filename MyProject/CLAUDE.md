Leia AGENTS.md

## Comandos

```bash
# Build (raiz do repo)
cd MyProject
dotnet build src/Backend/MyProject.slnx

# Testes
dotnet test src/Backend/MyProject.slnx

# Executar local (2 terminais)
# Terminal 1 — Host.Api (serviço interno, porta 5100)
dotnet run --project src/Backend/Host.Api --launch-profile http

# Terminal 2 — ApiGateway (proxy YARP, porta 5000)
dotnet run --project src/Backend/ApiGateway --launch-profile http

# Testar via gateway
curl -H "X-User-Id: dev-user" http://localhost:5000/api/catalog/products
```

## Pré-requisitos para execução

```bash
# 1. Subir PostgreSQL
docker run -d --name postgres-dev \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=myproject \
  -p 5432:5432 \
  postgres:17-alpine

# 2. Criar .env a partir do exemplo
cd src/Backend/Host.Api
cp .env.example .env

# 3. Executar (2 terminais)
dotnet run --launch-profile http                          # Terminal 1: Host.Api :5100
dotnet run --project src/Backend/ApiGateway --launch-profile http  # Terminal 2: Gateway :5000

# 4. Testar
curl -H "X-User-Id: dev-user" http://localhost:5000/api/catalog/products
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

## Portas

| Serviço | HTTP | HTTPS |
|---------|------|-------|
| ApiGateway (YARP) | 5000 | 5001 |
| Host.Api (interno) | 5100 | 5101 |
