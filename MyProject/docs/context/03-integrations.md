# 03 — Integrations

> Load this when: touching a contract between services.

## Diagram

```
┌──────────┐  HTTP  ┌──────────────┐  HTTP  ┌──────────┐  EF Core / Dapper  ┌────────────┐
│  Client  │───────▶│  ApiGateway  │───────▶│ Host.Api │───────────────────▶│ PostgreSQL │
└──────────┘        │  (YARP:5000) │        │ (:5100)  │                    └────────────┘
                    └──────────────┘        └──────────┘
                                                  │
                                                  │ GHCR (CD workflow)
                                                  ▼
                                            ┌──────────┐
                                            │ ghcr.io  │
                                            │  images  │
                                            └──────────┘
```

## Internal contracts

- **Gateway → Host.Api**: YARP routes `/api/*`, `/openapi/*`, `/scalar/*`, `/health*` to `http://localhost:5100/`. Headers forwarded transparently (including `X-User-Id` and `Authorization`).
- **Cross-module**: Domain events via Outbox pattern. Each module has its own `OutboxMessages` table. `OutboxBackgroundService<TContext>` processes events and dispatches to `IDomainEventHandler<T>` implementations. Modules do not reference each other.
- **Ownership**: `IHasOwner` interface marks entities that belong to a user. All write operations verify ownership before proceeding.

## External services

| Service | Used by | For what | Config location |
|---------|---------|----------|-----------------|
| PostgreSQL | Host.Api via Npgsql | Primary data store (catalog schema) | `.env` → `ConnectionStrings:Default` |
| NuGet.org | Build (dotnet restore) | Package dependencies | `Directory.Packages.props` |
| GitHub Container Registry (ghcr.io) | CD workflow | Container image hosting | `backend-cd.yml` (GITHUB_TOKEN secret) |
| GitHub Actions | CI/CD | Build, test, publish | `.github/workflows/` |

No external HTTP APIs, message brokers, or cloud services integrated yet. Gateway is the only externally exposed service.
