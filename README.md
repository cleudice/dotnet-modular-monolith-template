# .NET Modular Monolith Template

Open-source template for building **modular monolith** applications with .NET 10.0, Clean Architecture, and DDD tactical patterns.

## Features

- **.NET 10.0** with C# 13, file-scoped namespaces, primary constructors
- **Clean Architecture** — Domain / Application / Infrastructure per module
- **DDD** — AggregateRoot, ValueObjects (`readonly record struct`), Domain Events, Repository pattern
- **CQRS** — hand-rolled, no MediatR dependency
- **Minimal APIs** — no Controllers
- **EF Core** writes + **Dapper** reads
- **FluentValidation** at boundary + domain invariants
- **YARP** reverse proxy as API Gateway
- **JWT Bearer** authentication (with `X-User-Id` header fallback for dev)
- **Serilog** structured logging
- **Health checks** (liveness + readiness)
- **Rate limiting** (fixed window, configurable)
- **CORS** (dev open, production configurable)
- **Scalar** Swagger UI (dev) + OpenAPI spec (all environments)
- **Docker** multi-stage builds + docker-compose
- **GitHub Actions** CI/CD — build, test, publish to GHCR

## Architecture

```
Client
  │
  ▼
ApiGateway (:5000)  ─── YARP reverse proxy, TLS termination
  │
  ▼
Host.Api (:5100)     ─── Composition root, module wiring
  │
  ├── /api/catalog/*   Catalog module (full CRUD + ownership)
  ├── /api/ordering/*  Ordering module (placeholder)
  ├── /health           Liveness
  ├── /health/ready     Readiness (DB check)
  ├── /openapi/v1.json  OpenAPI spec
  └── /scalar/v1        Scalar UI (dev only)
```

### Project structure

```
MyProject/
├── src/Backend/
│   ├── Host.Api/                  Composition root
│   ├── ApiGateway/                YARP reverse proxy
│   ├── BuildingBlocks/            Shared kernel
│   │   ├── Domain/                Entity, ValueObject, AggregateRoot, IDomainEvent
│   │   ├── Application/           IUnitOfWork, IDomainEventDispatcher
│   │   └── Infrastructure/        AppDbContext, DomainEventDispatcher
│   └── Modules/
│       ├── Catalog/               Products CRUD (complete)
│       │   ├── Domain/            Product, ValueObjects, Exceptions
│       │   ├── Application/       Commands, Queries, DTOs, Validators
│       │   └── Infrastructure/    DbContext, Repository, Endpoints, DI
│       └── Ordering/              Placeholder
├── tests/Backend.Tests/           24+ unit + integration tests
├── docs/context/                  Reverse-engineered context pack (10 axes)
└── Directory.Build.props          Central build configuration
```

## Quick Start

### Prerequisites

- .NET SDK 10.0.108+
- Docker / Podman (for PostgreSQL)
- Or Docker Compose (for everything)

### Docker Compose (recommended)

```bash
docker compose up -d
# API ready at http://localhost:5000
# Scalar UI at http://localhost:5000/scalar/v1
curl http://localhost:5000/health
```

### Local development

```bash
# 1. Start PostgreSQL
docker run -d --name postgres-dev \
  -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=myproject -p 5432:5432 postgres:17-alpine

# 2. Configure environment
cd MyProject/src/Backend/Host.Api
cp .env.example .env
# Edit .env if needed

# 3. Run (2 terminals)
dotnet run --launch-profile http                          # Host.Api :5100
dotnet run --project ../ApiGateway --launch-profile http   # Gateway :5000
```

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/catalog/products` | List products (paginated) |
| `GET` | `/api/catalog/products/{id}` | Get product by ID |
| `POST` | `/api/catalog/products` | Create product |
| `PUT` | `/api/catalog/products/{id}` | Update product |
| `DELETE` | `/api/catalog/products/{id}` | Delete product |
| `PATCH` | `/api/catalog/products/{id}/stock?quantity=N` | Update stock |

### Authentication

```bash
# Dev: use X-User-Id header
curl -H "X-User-Id: my-user" http://localhost:5000/api/catalog/products

# Prod: JWT Bearer token (configure Jwt:Key in .env)
curl -H "Authorization: Bearer <token>" http://localhost:5000/api/catalog/products
```

## Build & Test

```bash
cd MyProject
dotnet build src/Backend/MyProject.slnx     # 0 warnings, 0 errors enforced
dotnet test src/Backend/MyProject.slnx       # 28+ tests
```

## Decisions (no going back)

- **No MediatR** — handlers injected directly
- **No AutoMapper/Mapster** — manual mapping via extension methods
- **No generic repository** — specific repository interfaces per aggregate
- **Value objects as `readonly record struct`** — self-validating, implicit operators
- **Typed domain exceptions** — mapped to HTTP via `IExceptionHandler`
- **No magic numbers** — constants on domain types (e.g. `ProductName.MaxLength`)

## Tech stack

| Tech | Version |
|------|---------|
| .NET SDK | 10.0.108 |
| EF Core | 10.0.9 |
| Dapper | 2.1.79 |
| Npgsql | 10.0.3 |
| FluentValidation | 12.1.1 |
| YARP | 2.3.0 |
| Serilog | 9.0.0 |
| Scalar | 2.1.0 |
| xUnit | 2.9.3 |
| FluentAssertions | 8.10.0 |

## License

MIT — use freely for your own projects.
