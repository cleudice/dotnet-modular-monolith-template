# 06 — Entry Points

> Load this when: locating where a flow begins.

## ApiGateway (YARP Reverse Proxy)

**Primary entry point.** Base URL: `http://localhost:5000` (HTTP) / `https://localhost:5001` (HTTPS)

All external traffic enters through the gateway. YARP reverse proxy routes `/api/{**catch-all}` to Host.Api (`http://localhost:5100/`). The gateway is a thin proxy — no business logic, no module references, no database access. TLS termination happens here.

Requests flow: Client → ApiGateway:5000 → Host.Api:5100 → Module endpoints

## Host.Api endpoints (internal)

Base URL: `http://localhost:5100` (HTTP only — HTTPS redirect removed; gateway handles TLS).

| Method | Route | Handler | Auth | What it does |
|--------|-------|---------|------|--------------|
| GET | `/api/catalog/products/{id}` | GetProductHandler | OwnerId header | Returns product if owned by caller |
| GET | `/api/catalog/products?page&pageSize` | ListProductsHandler | OwnerId header | Paginated list of caller's products |
| POST | `/api/catalog/products` | CreateProductHandler | OwnerId header | Creates product (validates + SKU uniqueness) |
| PUT | `/api/catalog/products/{id}` | UpdateProductHandler | OwnerId header | Updates name/description/price (ownership check) |
| DELETE | `/api/catalog/products/{id}` | DeleteProductHandler | OwnerId header | Deletes product (ownership check) |
| PATCH | `/api/catalog/products/{id}/stock?quantity=N` | UpdateProductStockHandler | OwnerId header | Adjusts stock quantity (ownership check) |

Owner identification: `X-User-Id` request header (temporary — replace with JWT claims when auth is implemented). Default: `"anonymous"`. YARP forwards all headers transparently, so the header reaches Host.Api unchanged.

## Health checks

| Endpoint | Purpose |
|----------|---------|
| `/health` | Liveness — app is alive |
| `/health/ready` | Readiness — DB connected, ready for traffic |

Both accessible via gateway (`http://localhost:5000/health`).

## API Documentation

| Resource | URL (via Gateway) | Dev | Produção |
|----------|-------------------|-----|----------|
| OpenAPI spec | `http://localhost:5000/openapi/v1.json` | ✅ | ✅ |
| Scalar UI | `http://localhost:5000/scalar/v1` | ✅ | ❌ |

**Produção**: OpenAPI spec exposto para contract publishing, CI/CD, integração. Scalar UI bloqueado — superfície de ataque desnecessária.

Gateway proxies `/openapi/*` and `/scalar/*` to Host.Api. A decisão de expor cada recurso é controlada no `Program.cs` do Host.Api (`MapOpenApi()` fora do `IsDevelopment()`, `MapScalarApiReference()` dentro).

## Jobs / events

None yet. Domain event dispatch is a stub in `AppDbContext.DispatchDomainEventsAsync()` — collects events from `AggregateRoot.DomainEvents`, clears them, but does not publish. Intended for future cross-module communication via in-process mediator or outbox pattern.
