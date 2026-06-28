# 09 — Maintenance & Gotchas

> Load this when: ANY change. Read the gotchas before touching the code.

## Known gotchas (most valuable — grows over time, never auto-overwrite)

- **Repository has zero git commits.** First commit should be a clean baseline of the current state.
- **No database migrations exist.** `CatalogDbContext.OnModelCreating` defines the schema, but `dotnet ef migrations add` has never been run. Any change to entity configuration needs a new migration.
- **`dotnet format` crashes on .NET 10.0** when writing changes (MSBuildWorkspace exception). Workaround: build with `EnforceCodeStyleInBuild=true` and fix IDE0011 warnings manually.
- **Domain event dispatch is a stub.** `AppDbContext.DispatchDomainEventsAsync()` collects and clears events but does nothing with them. When cross-module communication is needed, implement an `IDomainEventDispatcher` and inject it here.
- **`X-User-Id` header is temporary.** Replace with JWT `sub` claim when real authentication is implemented. The `GetOwnerId()` helper in `CatalogEndpoints` is the single point to change.
- **InMemory provider in tests does not enforce unique indexes.** `ProductRepositoryTests` passes but would fail against real PostgreSQL if SKU uniqueness is violated. Use Testcontainers for true integration tests.
- **Connection string in user-secrets only.** After cloning, developers must run `dotnet user-secrets set "ConnectionStrings:Default" "..."` before the app starts.

## Tech debt

| Area | Debt | Priority |
|------|------|----------|
| Ordering module | Class1.cs placeholders — no implementation | Low (template placeholder) |
| ApiGateway | Weather forecast boilerplate — should be YARP reverse proxy | Low |
| Docker | All Dockerfiles and docker-compose.yml are empty | Medium |
| CI/CD | GitHub Actions workflow files are empty | Medium |
| Frontend | package.json is empty — stack not chosen | Low |
| Health checks | Not implemented | Medium |
| CORS | Not configured | Medium (needed when frontend is added) |
| Auth | `X-User-Id` header — no real auth | High (before production) |
| Domain events | Dispatch is a stub | Medium (needed for cross-module features) |
| README | Does not exist | High (for open-source template) |

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

All packages are latest stable as of June 2026. Check for updates quarterly.
