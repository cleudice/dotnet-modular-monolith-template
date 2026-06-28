# Architecture Decisions

Recorded decisions for this modular monolith. Each section states the choice and the rationale.
Plan, implement, and review steps follow these conventions.

---

## 1. Project Layering — Clean Architecture

**Decision:** Clean Architecture with Domain / Application / Infrastructure layers per module.

```
Module.Domain          ← entities, value objects, domain interfaces. Zero dependencies.
Module.Application     ← use cases / handlers, orchestration. Depends on Domain + BuildingBlocks.
Module.Infrastructure  ← EF, repositories, external services. Depends inward.
Host.Api               ← composition root, wires DI, registers modules.
```

**Dependency rule:** `API → Application → Domain ← Infrastructure`. Infrastructure depends inward;
nothing depends on Infrastructure except the composition root.

**Shared kernel:** `BuildingBlocks.Domain`, `BuildingBlocks.Application`, `BuildingBlocks.Infrastructure`
hold cross-cutting base classes and contracts (Entity, ValueObject, AggregateRoot, domain event
interfaces, pipeline behaviors, outbox infrastructure).

**Rationale:** the template was scaffolded with this structure. It fits a multi-module, long-lived
codebase with rich domain logic.

---

## 2. Application Layer — Hybrid

**Decision:** plain application services for simple operations; CQRS-style handlers (command/query
separation) for complex flows. **No MediatR** — handlers are plain injected classes called directly
from endpoints, avoiding the MediatR license and dispatch indirection.

**Simple (Use Case / Service):**
```csharp
public class CatalogService(ICatalogRepository repo)
{
    public async Task<ProductDto?> GetAsync(int id, CancellationToken ct) =>
        (await repo.GetAsync(id, ct))?.ToDto();
}
```

**Complex (CQRS-style, hand-rolled):**
```csharp
public class PlaceOrderHandler(IOrderRepository orders, IPaymentGateway payments)
{
    public async Task<OrderDto> HandleAsync(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.Items);
        await payments.ChargeAsync(order.Total, ct);
        await orders.AddAsync(order, ct);
        return order.ToDto();
    }
}
```

**Cross-cutting concerns** (validation, logging, transaction scope) are added via decorator pattern
or a thin dispatch wrapper — not via MediatR pipeline behaviors.

**Rule of thumb:** start with a plain service. Extract a handler when:
- Read and write paths genuinely diverge
- The operation needs multiple cross-cutting concerns that a decorator earns its keep

**Rationale:** avoids the MediatR licensing issue, keeps simple things simple, and reserves structure
for where complexity actually lives.

---

## 3. API Style — Minimal APIs

**Decision:** Minimal APIs with endpoint groups organized per module and per feature.

**Organization convention:** each module exposes an extension method on `WebApplication` (or
`IEndpointRouteBuilder`) that registers its endpoints. `Program.cs` calls one method per module,
keeping the composition root clean.

```csharp
// In Catalog.Infrastructure:
public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/catalog").WithTags("Catalog");

        group.MapGet("/products/{id:int}", async (int id, CatalogService svc, CancellationToken ct) =>
        {
            var product = await svc.GetAsync(id, ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        return routes;
    }
}

// In Host.Api Program.cs:
app.MapCatalogEndpoints();
app.MapOrderingEndpoints();
```

**Rationale:** concise, AOT-friendly, pairs well with Vertical Slice features inside modules. The
explicit dependency injection per endpoint (no "magic" model binding) makes dependencies visible.

---

## 4. Persistence — EF Core (writes) + Dapper (reads)

**Decision:** EF Core for write operations (commands, domain persistence) and Dapper for read
operations (queries, projections, reporting). Both share the same database.

**Repository pattern:** specific repositories (`IOrderRepository`) defined in the Domain layer and
implemented in Infrastructure. The interface only has meaningful domain methods — no generic
`IRepository<T>` boilerplate.

**EF Core for writes:**
```csharp
public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken ct) => await db.Orders.AddAsync(order, ct);
    public async Task<Order?> GetAsync(int id, CancellationToken ct) => await db.Orders.FindAsync([id], ct);
}
```

**Dapper for reads:**
```csharp
public class OrderQueries(IDbConnectionFactory factory) : IOrderQueries
{
    public async Task<OrderDto?> GetAsync(int id, CancellationToken ct)
    {
        using var conn = factory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<OrderDto>(
            "SELECT Id, CustomerId, Total FROM Orders WHERE Id = @Id", new { Id = id });
    }
}
```

**Rationale:** EF provides change tracking, migrations, and domain-object persistence for the write
path. Dapper gives predictable, fast SQL for read projections without the translation overhead.
This is the natural pairing for command/query separation.

---

## 5. Validation — FluentValidation (boundary) + Domain Invariants

**Decision:** FluentValidation for input validation at the API boundary; domain-enforced invariants
inside entities and value objects.

**Boundary (FluentValidation):**
```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100);
    }
}
```

**Domain invariants:**
```csharp
public class Order
{
    public static Order Create(string customerId, IReadOnlyList<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException(nameof(customerId));
        if (items.Count == 0)
            throw new DomainException("Order must have at least one item.");
        return new Order { CustomerId = customerId, Items = items.ToList() };
    }
}
```

**Separation:** FluentValidation answers "is the request well-formed?" (→ 400). Domain invariants
answer "is this allowed?" (→ domain exception). Different concerns, different layers.

**Rationale:** FluentValidation handles conditional and async rules cleanly. Domain invariants
guarantee entities can never enter an invalid state from any caller — the strongest guarantee.

---

## 6. Object Mapping — Manual

**Decision:** hand-written mapping methods. No AutoMapper, no Mapster. Zero mapping dependencies.

```csharp
public static class OrderMappings
{
    public static OrderDto ToDto(this Order order) => new(
        order.Id,
        order.CustomerId,
        order.Total,
        order.Items.Select(i => i.ToDto()).ToList());
}
```

**Rationale:** explicit, compile-time safe, AOT-friendly, zero reflection cost, no licensing
concerns. The boilerplate is bounded — extensions methods and record constructors keep it concise.
A renamed property breaks the build immediately at the mapping site.

---

## Summary

| Axis | Choice |
|------|--------|
| Layering | Clean Architecture (Domain / Application / Infrastructure per module) |
| Application Layer | Hybrid — services for simple, CQRS-style handlers for complex. No MediatR |
| API Style | Minimal APIs, grouped per module via extension methods |
| Persistence | EF Core (writes) + Dapper (reads). Specific repositories, no generic `IRepository<T>` |
| Validation | FluentValidation at boundary + domain invariants in entities |
| Mapping | Manual — extension methods and record constructors |
