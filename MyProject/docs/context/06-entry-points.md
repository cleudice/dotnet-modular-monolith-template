# 06 — Entry Points

> Load this when: locating where a flow begins.

## Host.Api endpoints

Base URL: `http://localhost:5085` (HTTP) / `https://localhost:7195` (HTTPS)

| Method | Route | Handler | Auth | What it does |
|--------|-------|---------|------|--------------|
| GET | `/api/catalog/products/{id}` | GetProductHandler | OwnerId header | Returns product if owned by caller |
| GET | `/api/catalog/products?page&pageSize` | ListProductsHandler | OwnerId header | Paginated list of caller's products |
| POST | `/api/catalog/products` | CreateProductHandler | OwnerId header | Creates product (validates + SKU uniqueness) |
| PUT | `/api/catalog/products/{id}` | UpdateProductHandler | OwnerId header | Updates name/description/price (ownership check) |
| DELETE | `/api/catalog/products/{id}` | DeleteProductHandler | OwnerId header | Deletes product (ownership check) |
| PATCH | `/api/catalog/products/{id}/stock?quantity=N` | UpdateProductStockHandler | OwnerId header | Adjusts stock quantity (ownership check) |

Owner identification: `X-User-Id` request header (temporary — replace with JWT claims when auth is implemented). Default: `"anonymous"`.

## ApiGateway endpoints

Base URL: `http://localhost:5019`

| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/weatherforecast` | Boilerplate sample from `dotnet new` — not production |

## Jobs / events

None yet. Domain event dispatch is a stub in `AppDbContext.DispatchDomainEventsAsync()` — collects events from `AggregateRoot.DomainEvents`, clears them, but does not publish. Intended for future cross-module communication via in-process mediator or outbox pattern.
