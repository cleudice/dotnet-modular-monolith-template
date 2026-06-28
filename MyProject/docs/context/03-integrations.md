# 03 — Integrations

> Load this when: touching a contract between services.

## Diagram

```
┌──────────┐  HTTP (OpenAPI)  ┌──────────┐  EF Core / Npgsql  ┌────────────┐
│  Client  │─────────────────▶│ Host.Api │──────────────────▶│ PostgreSQL │
└──────────┘                  └──────────┘                    └────────────┘
                                    │
                                    │ (future)
                                    ▼
                              ┌──────────┐
                              │ ApiGateway│
                              │  (YARP?) │
                              └──────────┘
```

## Internal contracts

No inter-module contracts yet. Domain event dispatch is a stub in `AppDbContext` — events are collected and cleared, but not published. Cross-module communication will use in-process domain events when implemented.

## External services

| Service | Used by | For what | Sandbox/ref |
|---------|---------|----------|-------------|
| PostgreSQL | Host.Api via Npgsql | Primary data store | Connection string in user-secrets |
| NuGet.org | Build (dotnet restore) | Package dependencies | `Directory.Packages.props` |

No external HTTP APIs, message brokers, or cloud services are integrated yet.
