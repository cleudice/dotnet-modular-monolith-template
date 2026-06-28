# 09 — Maintenance & Gotchas

> Load this when: ANY change. Read the gotchas before touching the code.

## Known gotchas (most valuable — grows over time, never auto-overwrite)

- **Repository has zero git commits.** First commit should be a clean baseline of the current state.
- **No database migrations exist.** `CatalogDbContext.OnModelCreating` defines the schema, but `dotnet ef migrations add` has never been run. Any change to entity configuration needs a new migration.
- **`dotnet format` crashes on .NET 10.0** when writing changes (MSBuildWorkspace exception). Workaround: build with `EnforceCodeStyleInBuild=true` and fix IDE0011 warnings manually.
- **Domain event dispatch is a stub.** `AppDbContext.DispatchDomainEventsAsync()` collects and clears events but does nothing with them. When cross-module communication is needed, implement an `IDomainEventDispatcher` and inject it here.
- **`X-User-Id` header is temporary.** Replace with JWT `sub` claim when real authentication is implemented. The `GetOwnerId()` helper in `CatalogEndpoints` is the single point to change.
- **InMemory provider in tests does not enforce unique indexes.** `ProductRepositoryTests` passes but would fail against real PostgreSQL if SKU uniqueness is violated. Use Testcontainers for true integration tests.
- **Connection string via `.env` file.** Copy `.env.example` to `.env` in `src/Backend/Host.Api/`. Falls back to user-secrets or environment variables. The `.env` file is gitignored.
- **`TreatWarningsAsErrors=true` enforces zero-warning builds.** CA1707 (test name underscores) suppressed in `Backend.Tests.csproj`. CA1848 (LoggerMessage delegates) suppressed in `Host.Api.csproj`. Add new suppressions sparingly — fix the code first.

## Tech debt

| Area | Debt | Priority |
|------|------|----------|
| ApiGateway | ~~Weather forecast boilerplate~~ ✅ YARP 2.3.0, routes `/api/*`, `/openapi/*`, `/scalar/*` to Host.Api | — |
| Docker | ~~Empty~~ ✅ Multi-stage Dockerfiles (Host.Api, ApiGateway), docker-compose.yml funcional | — |
| CI/CD | ~~Empty~~ ✅ backend-ci.yml (build+test), backend-cd.yml (publish GHCR) | — |
| Frontend | package.json is empty — stack not chosen | Low |
| Health checks | ~~Not implemented~~ ✅ /health (liveness), /health/ready (readiness + DB) | — |
| CORS | ~~Not configured~~ ✅ Dev: AllowAny, Prod: configurable origins | — |
| Auth | ~~`X-User-Id` header~~ ✅ JWT Bearer + X-User-Id fallback | — |
| Domain events | ~~Dispatch is a stub~~ ✅ Outbox pattern — per-module table, shared BackgroundService, serializa JSON | — |
| README | ~~Does not exist~~ ✅ README.md at repo root | — |

## Versions & support

| Component | Version | Notes |
|-----------|---------|-------|
| .NET SDK | 10.0.108 | Pinned in `global.json` |
| Target framework | net10.0 | All projects |
| PostgreSQL (Npgsql) | 10.0.3 | |
| EF Core | 10.0.9 | |
| Dapper | 2.1.79 | |
| FluentValidation | 12.1.1 | |
| FluentAssertions | 8.10.0 | |
| xUnit | 2.9.3 | |
| YARP | 2.3.0 | Reverse proxy |
| Scalar | 2.1.0 | API docs UI |
| Serilog | 9.0.0 | Structured logging |

All packages are latest stable as of June 2026. Check for updates quarterly.
