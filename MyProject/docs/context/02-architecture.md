# 02 — Architecture

> Load this when: designing or reviewing a solution.

## Per subsystem

### Catalog Module (reference implementation)

| Layer | Project | Purpose | Dependencies |
|-------|---------|---------|--------------|
| Domain | `Catalog.Domain` | Product aggregate, value objects, repository interface, domain exceptions | BuildingBlocks.Domain |
| Application | `Catalog.Application` | Commands, queries, handlers, DTOs, validators, mappings | Domain + BuildingBlocks.Application |
| Infrastructure | `Catalog.Infrastructure` | EF DbContext, repository impl, Dapper queries, Minimal API endpoints, DI registration | Domain + Application + BuildingBlocks.Infrastructure |

**Patterns (with evidence):**

- **Clean Architecture** — `Catalog.Domain` has zero external deps (see `.csproj`). `Catalog.Infrastructure` depends inward on Domain interfaces it implements. Composition root (`Program.cs`) is the only place that wires them together.
- **DDD Tactical** — `Product` extends `AggregateRoot` (from BuildingBlocks). `ProductName`, `Sku`, `Price` are `readonly record struct` value objects with self-validation. `IProductRepository` is defined in Domain, implemented in Infrastructure.
- **CQRS (hand-rolled, no MediatR)** — Commands (`CreateProductCommand`, `UpdateProductCommand`, `DeleteProductCommand`) and Queries (`GetProductQuery`, `ListProductsQuery`) are plain records. Handlers are plain injected classes called directly from endpoints. No mediator dispatch — avoids the MediatR commercial license.
- **EF Core (writes) + Dapper (reads)** — `ProductRepository` uses EF for commands. `ProductQueries` uses Dapper for reads via `IDbConnectionFactory`.
- **FluentValidation + Domain Invariants** — `CreateProductValidator` validates at the boundary (→ 400). Value object constructors enforce invariants (→ domain exception).
- **Typed Exceptions** — `CatalogValidationException` (400), `CatalogNotFoundException` (404), `CatalogForbiddenException` (403). Mapped by `DomainExceptionHandler` (IExceptionHandler middleware).
- **Manual Mapping** — `ProductMappings.ToDto()` extension method. No AutoMapper, no Mapster.

### Ordering Module (placeholder)

All three projects contain only `Class1.cs` stubs. Follow the Catalog pattern when implementing.

### BuildingBlocks (shared kernel)

| Project | Contents |
|---------|----------|
| `BuildingBlocks.Domain` | `Entity` (id-based equality), `ValueObject` (structural equality), `AggregateRoot` (domain event collection), `IDomainEvent`, `DomainException`, `IHasOwner` |
| `BuildingBlocks.Application` | `IUnitOfWork` (transaction boundary) |
| `BuildingBlocks.Infrastructure` | `AppDbContext` (base EF context with domain event dispatch stub), `IDbConnectionFactory`, `NpgsqlConnectionFactory` |

### Host.Api (composition root)

`Program.cs` calls `AddCatalogModule()` (DI registration) and `MapCatalogEndpoints()` (route registration). Each future module gets its own pair. Exception handling via `UseExceptionHandler()` with `DomainExceptionHandler`.

### ApiGateway (placeholder)

Currently the .NET weather forecast boilerplate. Intended as a separate BFF/reverse proxy in front of Host.Api — not yet configured with YARP or similar.

## Decisions & trade-offs

Recorded in `docs/context/conventions.md`. Key choices: Clean Architecture, Hybrid CQRS without MediatR, Minimal APIs, EF (writes) + Dapper (reads), FluentValidation + Domain Invariants, Manual mapping.
