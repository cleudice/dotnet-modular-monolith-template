# 02 — Architecture

> Load this when: designing or reviewing a solution.

## Per subsystem

### Catalog Module (reference implementation)

| Layer | Project | Purpose | Dependencies |
|-------|---------|---------|--------------|
| Domain | `Catalog.Domain` | Product aggregate, value objects (ProductName, Sku, Price), repository interface, domain exceptions | BuildingBlocks.Domain |
| Application | `Catalog.Application` | Commands, queries, handlers, DTOs, validators, mappings | Domain + BuildingBlocks.Application |
| Infrastructure | `Catalog.Infrastructure` | EF DbContext (Products + OutboxMessages), repository impl, Dapper queries, Minimal API endpoints, DI registration | Domain + Application + BuildingBlocks.Infrastructure |

**Patterns (with evidence):**

- **Clean Architecture** — `Catalog.Domain` has zero external deps (see `.csproj`). Infrastructure depends inward on Domain interfaces.
- **DDD Tactical** — `Product` extends `AggregateRoot`. `ProductName`, `Sku`, `Price` are `readonly record struct` value objects with self-validation. `IProductRepository` defined in Domain, implemented in Infrastructure.
- **CQRS (hand-rolled, no MediatR)** — Commands and Queries are plain records. Handlers are injected classes called directly from endpoints. No mediator dispatch.
- **EF Core (writes) + Dapper (reads)** — `ProductRepository` uses EF for commands. `ProductQueries` uses Dapper via `IDbConnectionFactory`.
- **FluentValidation + Domain Invariants** — `CreateProductValidator` validates at boundary (→ 400). Value object constructors enforce invariants (→ domain exception).
- **Outbox Pattern** — Domain events raised by aggregates are captured by `AppDbContext.CaptureOutboxMessages()`, serialized to JSON, and persisted to the module's `OutboxMessages` table in the same transaction. `OutboxBackgroundService<CatalogDbContext>` polls and dispatches via `IDomainEventDispatcher`.
- **Typed Exceptions** — `CatalogValidationException` (400), `CatalogNotFoundException` (404), `CatalogForbiddenException` (403). Mapped by `DomainExceptionHandler` (IExceptionHandler middleware).
- **Manual Mapping** — `ProductMappings.ToDto()` extension method. No AutoMapper/Mapster.

### Adding a new module

Run `./scripts/add-module.sh <ModuleName>` to scaffold a new module from the Catalog pattern. The script copies the project structure, updates the solution file, and adds the Host.Api reference. See `10-feature-guide.md` for the manual steps if preferred.

### BuildingBlocks (shared kernel)

| Project | Contents |
|---------|----------|
| `BuildingBlocks.Domain` | `Entity` (id-based equality), `ValueObject` (structural equality), `AggregateRoot` (domain event collection), `IDomainEvent`, `DomainException`, `IHasOwner` |
| `BuildingBlocks.Application` | `IUnitOfWork`, `IDomainEventDispatcher`, `IDomainEventHandler<T>` |
| `BuildingBlocks.Infrastructure` | `AppDbContext` (base EF context, `CaptureOutboxMessages`), `OutboxMessage`, `OutboxBackgroundService<TContext>`, `IDbConnectionFactory`, `NpgsqlConnectionFactory`, `DomainEventDispatcher` |

### Host.Api (composition root)

`Program.cs` wires everything:
- **Serilog** structured logging (JSON in prod, readable in dev)
- **JWT Bearer** auth (optional — falls back to `X-User-Id` header if no `Jwt:Key`)
- **Rate Limiting** (fixed window: 100 req/min)
- **CORS** (dev: AllowAny, prod: configurable origins)
- **Response Compression** (gzip/brotli)
- **Health Checks** (`/health` liveness, `/health/ready` + DB)
- **OpenAPI** spec (all envs) + Scalar UI (dev only)
- Module DI via `AddCatalogModule()` and endpoints via `MapCatalogEndpoints()`

### ApiGateway (YARP Reverse Proxy)

Primary entry point (ports 5000/5001). Thin proxy — zero module references, zero business logic. Routes:

| Path pattern | Destination |
|-------------|-------------|
| `/api/{**catch-all}` | Host.Api:5100 |
| `/openapi/{**catch-all}` | Host.Api:5100 |
| `/scalar/{**catch-all}` | Host.Api:5100 |
| `/health{**catch-all}` | Host.Api:5100 |

## Decisions & trade-offs

| Decision | Rationale |
|----------|-----------|
| Clean Architecture | Per-module separation, testable, no cross-module coupling |
| CQRS without MediatR | Avoids commercial license, handlers are plain DI |
| Minimal APIs | Lighter than Controllers, .NET 10 native |
| EF (writes) + Dapper (reads) | EF for rich domain model, Dapper for perf queries |
| FluentValidation + Domain Invariants | Boundary validation + self-validating value objects |
| Manual mapping | No reflection overhead, compile-safe |
| Outbox (per module) | Shared logic in BuildingBlocks, per-module table isolation |
| YARP (no module refs) | Gateway stays thin, no rebuild when modules change |
| Serilog (structured JSON) | Container-friendly, queryable in log aggregators |
