# 04 — Data Flow

> Load this when: touching DB, repository, or query.

## Stores

| Store | Used by | Access pattern |
|-------|---------|----------------|
| PostgreSQL (schema: `catalog`) | Catalog module | EF Core for writes, Dapper for reads |
| PostgreSQL (schema: `catalog`) | Outbox processor | Poll `OutboxMessages`, mark processed |

## Key entities

### `catalog.Products`

| Column | Type | Constraints |
|--------|------|-------------|
| Id | bigint | PK, auto-increment |
| Name | nvarchar(200) | NOT NULL |
| Description | nvarchar(1000) | nullable |
| Price | decimal(18,2) | NOT NULL |
| Sku | nvarchar(50) | NOT NULL, UNIQUE INDEX |
| StockQuantity | int | NOT NULL |
| OwnerId | nvarchar(100) | NOT NULL, INDEX (OwnerId, Id) |

**Access patterns:**
- **Write path:** `ProductRepository` (EF Core) — `AddAsync`, `Update`, `Delete`
- **Read path:** `ProductQueries` (Dapper) — `GetAsync`, `ListAsync` (by OwnerId, paginated)
- **Ownership filter:** All list queries filter by `OwnerId`. Composite index `(OwnerId, Id)`.
- **SKU lookup:** `GetBySkuAsync` normalizes input (`Trim().ToUpperInvariant()`). Unique index.

### `catalog.OutboxMessages`

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| Type | nvarchar(500) | NOT NULL (full CLR type name) |
| Payload | text | NOT NULL (JSON serialized event) |
| OccurredOn | timestamp | NOT NULL |
| ProcessedOn | timestamp | nullable (NULL = pending) |
| Error | text | nullable |

**Access pattern:**
- **Write:** `AppDbContext.CaptureOutboxMessages()` serializes domain events → `Set<OutboxMessage>().Add()`. Saved in same transaction as entity changes.
- **Read/Update:** `OutboxBackgroundService<CatalogDbContext>` polls `WHERE ProcessedOn IS NULL` every 5s, dispatches via `IDomainEventDispatcher`, updates `ProcessedOn`/`Error`.
- **Index:** `ProcessedOn` (for polling query).

## Outbox flow

```
Product.Update() → AddDomainEvent(ProductUpdatedEvent)
       │
       ▼
CatalogDbContext.SaveChangesAsync()
       │
       ├─▸ AppDbContext.CaptureOutboxMessages()
       │     └─▸ Set<OutboxMessage>().Add(FromDomainEvent(e))
       │
       └─▸ base.SaveChangesAsync()  ← Same transaction
              │
              ▼
         [Transaction committed: Products updated + OutboxMessages inserted]
              │
              ▼  (async, up to 5s later)
OutboxBackgroundService<CatalogDbContext>.ProcessBatchAsync()
       │
       ├─▸ SELECT * FROM OutboxMessages WHERE ProcessedOn IS NULL
       ├─▸ JsonSerializer.Deserialize(eventType) → IDomainEvent
       ├─▸ IDomainEventDispatcher.DispatchAsync() → IDomainEventHandler<T>.HandleAsync()
       └─▸ MarkProcessed() + SaveChangesAsync()
```

## Migrations

No EF Core migrations generated — schema created via `EnsureCreated()` in Development. For production, generate migrations:

```bash
cd src/Backend
dotnet ef migrations add InitialCreate --project Modules/Catalog/Catalog.Infrastructure --startup-project Host.Api
dotnet ef database update
```

Connection string: `ConnectionStrings:Default` from `.env` (dev) or environment variables (prod).
