# 06 — Entry Points

> Load this when: locating where a flow begins.

## ApiGateway (YARP Reverse Proxy)

**Primary entry point.** Base URL: `http://localhost:5019` (HTTP) / `https://localhost:7227` (HTTPS)

All external traffic enters through the gateway. YARP reverse proxy routes `/api/{**catch-all}` to Host.Api (`http://localhost:5085/`). The gateway is a thin proxy — no business logic, no module references, no database access. TLS termination happens here.

Requests flow: Client → ApiGateway:5019 → Host.Api:5085 → Module endpoints

## Host.Api endpoints (internal)

Base URL: `http://localhost:5085` (HTTP only — HTTPS redirect removed; gateway handles TLS).

| Method | Route | Handler | Auth | What it does |
|--------|-------|---------|------|--------------|
| GET | `/api/catalog/products/{id}` | GetProductHandler | OwnerId header | Returns product if owned by caller |
| GET | `/api/catalog/products?page&pageSize` | ListProductsHandler | OwnerId header | Paginated list of caller's products |
| POST | `/api/catalog/products` | CreateProductHandler | OwnerId header | Creates product (validates + SKU uniqueness) |
| PUT | `/api/catalog/products/{id}` | UpdateProductHandler | OwnerId header | Updates name/description/price (ownership check) |
| DELETE | `/api/catalog/products/{id}` | DeleteProductHandler | OwnerId header | Deletes product (ownership check) |
| PATCH | `/api/catalog/products/{id}/stock?quantity=N` | UpdateProductStockHandler | OwnerId header | Adjusts stock quantity (ownership check) |

Owner identification: `X-User-Id` request header (temporary — replace with JWT claims when auth is implemented). Default: `"anonymous"`. YARP forwards all headers transparently, so the header reaches Host.Api unchanged.

## Jobs / events

None yet. Domain event dispatch is a stub in `AppDbContext.DispatchDomainEventsAsync()` — collects events from `AggregateRoot.DomainEvents`, clears them, but does not publish. Intended for future cross-module communication via in-process mediator or outbox pattern.
