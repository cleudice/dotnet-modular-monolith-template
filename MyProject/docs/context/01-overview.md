# 01 — Overview

> Load this when: first contact with the project.

## What it does

This is an **open-source template** for building modular monoliths in .NET 10.0. It provides a pre-configured architecture (Clean Architecture + DDD + CQRS + Outbox) so developers can start a new project with a solid foundation. The Catalog module serves as a reference implementation.

## Subsystems / repositories

| Path/Repo | Role | Stack | Key tech |
|-----------|------|-------|----------|
| src/Backend/Host.Api | Composition root | ASP.NET Core 10.0 Minimal API | Serilog, JWT, HealthChecks, RateLimiting, CORS, Compression |
| src/Backend/ApiGateway | Reverse proxy | ASP.NET Core 10.0 + YARP 2.3.0 | Routes /api/*, /openapi/*, /scalar/*, /health* → Host.Api |
| src/Backend/BuildingBlocks | Shared kernel | .NET 10.0 class libraries | Entity, ValueObject, AggregateRoot, OutboxMessage, OutboxBackgroundService<T> |
| src/Backend/Modules/Catalog | Product catalog (reference) | .NET 10.0 class libraries | Full CRUD + ownership, OutboxMessages table |
| src/Backend/Modules/Ordering | Order management (placeholder) | .NET 10.0 class libraries | Class1.cs stubs |
| src/Frontend | Frontend (placeholder) | Node/JS | Empty package.json |
| tests/Backend.Tests | Test suite | xUnit + FluentAssertions + Testcontainers | 24 unit (InMemory) + 4 integration (real PostgreSQL) |
| tests/Frontend.Tests | Frontend test placeholder | xUnit | — |

## Mental map

```
                          ┌─────────────────────────────────┐
  Client ────▶ ApiGateway │          Host.Api (:5100)       │
              (:5000)     │  ┌───────────────────────────┐  │
              YARP        │  │     Catalog Module         │  │
                          │  │  Domain → App → Infra      │  │
                          │  │  Products + OutboxMessages │  │
                          │  └───────────────────────────┘  │
                          │  ┌───────────────────────────┐  │
                          │  │    Ordering Module         │  │
                          │  │  Domain → App → Infra      │  │
                          │  │    (placeholder)           │  │
                          │  └───────────────────────────┘  │
                          │  ┌───────────────────────────┐  │
                          │  │      BuildingBlocks        │  │
                          │  │  Domain → App → Infra      │  │
                          │  │  (Outbox, base classes)    │  │
                          │  └───────────────────────────┘  │
                          └──────────────┬──────────────────┘
                                         │
                                         ▼
                              ┌─────────────────────┐
                              │    PostgreSQL       │
                              │  catalog.Products   │
                              │  catalog.OutboxMsg  │
                              └─────────────────────┘
```

- ApiGateway is the single entry point — TLS termination, YARP reverse proxy
- Host.Api registers all modules via DI, runs internally (HTTP only, behind proxy)
- Each module is self-contained with Domain / Application / Infrastructure layers
- BuildingBlocks provides shared kernel: base classes + OutboxMessage + OutboxBackgroundService<T>
- Cross-module communication: domain events via Outbox (per-module tables, shared BackgroundService)
- JWT auth with X-User-Id header fallback for dev convenience
