# 04 — Data Flow

> Load this when: touching DB, repository, or query.

## Stores

| Store | Used by | Access pattern |
|-------|---------|----------------|
| PostgreSQL (schema: `catalog`) | Catalog module | EF Core for writes, Dapper for reads |

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
- **Write path:** `ProductRepository` (EF Core) — `AddAsync`, `Update` (attached), `Delete` (removed)
- **Read path:** `ProductQueries` (Dapper) — `GetAsync` (by Id), `ListAsync` (by OwnerId, paginated)
- **Ownership filter:** All list queries filter by `OwnerId`. The `(OwnerId, Id)` composite index supports this.
- **SKU lookup:** `GetBySkuAsync` with normalized SKU (`Trim().ToUpperInvariant()`). Unique index on `Sku`.

## Migrations

No migrations have been generated yet. The `CatalogDbContext.OnModelCreating` defines the schema via Fluent API. To create the initial migration:

```bash
cd src/Backend
dotnet ef migrations add InitialCreate --project Modules/Catalog/Catalog.Infrastructure --startup-project Host.Api
dotnet ef database update --project Modules/Catalog/Catalog.Infrastructure --startup-project Host.Api
```

EF Core provider: `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.2. Connection string from `IConfiguration.GetConnectionString("Default")` — stored via `dotnet user-secrets` in development.
