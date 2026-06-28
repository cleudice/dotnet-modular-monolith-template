# 01 — Overview

> Load this when: first contact with the project.

## What it does

This is an **open-source template** for building modular monoliths in .NET. It provides a pre-configured architecture (Clean Architecture + DDD + CQRS) so developers can start a new project with a solid foundation instead of scaffolding from scratch. The Catalog module serves as a reference implementation demonstrating every architectural pattern in action.

## Subsystems / repositories

| Path/Repo | Role | Stack | Subsystem |
|-----------|------|-------|-----------|
| src/Backend/Host.Api | Composition root | ASP.NET Core 10.0 Minimal API | Modern |
| src/Backend/ApiGateway | API gateway (placeholder) | ASP.NET Core 10.0 Minimal API | Modern |
| src/Backend/BuildingBlocks | Shared kernel | .NET 10.0 class libraries | Modern |
| src/Backend/Modules/Catalog | Product catalog module | .NET 10.0 class libraries | Modern |
| src/Backend/Modules/Ordering | Order management (placeholder) | .NET 10.0 class libraries | Modern |
| src/Frontend | Frontend (placeholder) | Node/JS (not yet chosen) | Modern |
| tests/Backend.Tests | Backend test suite | xUnit + FluentAssertions | Modern |
| tests/Frontend.Tests | Frontend test placeholder | xUnit | Modern |

## Mental map

```
┌──────────────┐     ┌─────────────────────────────────┐
│  ApiGateway  │────▶│           Host.Api              │
│  (placeholder)│     │  ┌───────────────────────────┐ │
└──────────────┘     │  │     Catalog Module         │ │
                     │  │  Domain → App → Infra      │ │
                     │  └───────────────────────────┘ │
                     │  ┌───────────────────────────┐ │
                     │  │    Ordering Module         │ │
                     │  │  Domain → App → Infra      │ │
                     │  │    (placeholder)           │ │
                     │  └───────────────────────────┘ │
                     │  ┌───────────────────────────┐ │
                     │  │      BuildingBlocks        │ │
                     │  │  Domain → App → Infra      │ │
                     │  │    (shared kernel)         │ │
                     │  └───────────────────────────┘ │
                     └─────────────────────────────────┘
                                │
                                ▼
                     ┌─────────────────────┐
                     │    PostgreSQL       │
                     └─────────────────────┘
```

- Host.Api is the monolith entry point — it registers all modules via DI
- Each module is self-contained with its own Domain / Application / Infrastructure layers
- BuildingBlocks provides shared base classes (Entity, ValueObject, AggregateRoot) and infrastructure (AppDbContext, IDbConnectionFactory)
- Modules do not reference each other — cross-module communication goes through domain events (stub in AppDbContext)
