# 10 — Feature Guide

> Load this when: adding a feature.

## Recipe: adding a new business module

**Quick:** `./scripts/add-module.sh <ModuleName>` automates steps 1–5 below.

### 1. Create projects
```
Modules/NewModule/
├── NewModule.Domain/          ← depends on BuildingBlocks.Domain
├── NewModule.Application/     ← depends on NewModule.Domain + BuildingBlocks.Application
└── NewModule.Infrastructure/  ← depends on NewModule.* + BuildingBlocks.Infrastructure
```

Add to `MyProject.slnx` and reference from `Host.Api.csproj`.

### 2. Domain layer
- `Entities/` — aggregate root extending `AggregateRoot`, implementing `IHasOwner` if owned
- `ValueObjects/` — `readonly record struct` with self-validation
- `Exceptions/` — typed exceptions extending `DomainException`
- `Repositories/` — specific repository interface (not generic `IRepository<T>`)

### 3. Application layer
- `Commands/` — `record CommandName(...)` + `CommandNameHandler(...)` with `HandleAsync`
- `Queries/` — `record QueryName(...)` + `QueryNameHandler(...)` with `HandleAsync`
- `Dtos/` — response DTOs as `record`
- `Mappings/` — `ToDto()` extension methods (manual mapping)
- `Validation/` — FluentValidation validators per command

### 4. Infrastructure layer
- `Data/` — `DbContext` extending `AppDbContext`, repository implementing domain interface, Dapper queries class. Add `DbSet<OutboxMessage>` and configure `modelBuilder.Entity<OutboxMessage>()` for the module's outbox table.
- `Endpoints/` — `MapNewModuleEndpoints(this IEndpointRouteBuilder)` with Minimal API group
- `DependencyInjection/` — `AddNewModuleModule(this IServiceCollection, IConfiguration)` registering all DI, plus `AddHostedService<OutboxBackgroundService<NewModuleDbContext>>()`

### 5. Wire up in Host.Api
```csharp
// Program.cs
using Modules.NewModule.Infrastructure;
builder.Services.AddNewModuleModule(builder.Configuration);
// ...
app.MapNewModuleEndpoints();
```

### 6. Tests
- `tests/Backend.Tests/NewModule/Domain/` — entity and value object tests
- `tests/Backend.Tests/NewModule/Application/` — validator tests
- `tests/Backend.Tests/NewModule/Infrastructure/` — repository integration tests (InMemory + Testcontainers PostgreSQL)

### 7. Domain events (if module needs to publish events)
1. Define event: `record SomethingHappened(...) : IDomainEvent`
2. Raise in aggregate: `AddDomainEvent(new SomethingHappened(...))`
3. SaveChanges captures it into `OutboxMessages` automatically (via `AppDbContext.CaptureOutboxMessages()`)
4. Handler in another module: `class WhenSomethingHappened : IDomainEventHandler<SomethingHappened> { ... }`
5. Handler auto-discovered by `DomainEventDispatcher` via `IServiceProvider`

## Cross-cutting checklist

- [ ] Doesn't break a deployed contract (no existing endpoints changed/removed)
- [ ] Follows conventions (08-conventions.md) — naming, folders, patterns
- [ ] Covers security (07-security.md) — ownership check, parameterized queries, no secrets in config
- [ ] Avoids a known gotcha (09-maintenance.md) — runs migrations, respects domain event stub, uses `X-User-Id` extraction point
- [ ] `dotnet build MyProject.slnx` — 0 errors, 0 warnings
- [ ] `dotnet test` — all tests pass, new tests added for new module
- [ ] Connection string in user-secrets, not committed
