# 05 — Personas & Scenarios

> Load this when: understanding a business requirement.

## Personas

| Persona | Who | What they do |
|---------|-----|--------------|
| Developer | User of the template | Uses this project as starting point for modular monolith. Reads Catalog module as reference, copies pattern for new modules. |
| Product Owner | Owner of products in Catalog | Creates, updates, deletes their own products. Only sees and manages their own catalog. |

## Key journeys

1. **Developer onboards** → clones repo → `cp .env.example .env` + Docker PostgreSQL → `dotnet run` → Catalog API at `http://localhost:5000/api/catalog/products`
2. **Developer adds a module** → copies Catalog structure → creates Domain/Application/Infrastructure projects → `AddXxxModule()` + `MapXxxEndpoints()` in `Program.cs` → registers outbox table + background service
3. **Product Owner manages catalog** → authenticates via JWT (or `X-User-Id` header in dev) → POST creates product → GET lists own products → PUT updates → DELETE removes → PATCH adjusts stock
4. **Cross-owner isolation** → Owner A can't see/modify Owner B's products. Returns 403 (Forbidden) for write attempts, 404 (Not Found) for GET to avoid leaking existence.
5. **Developer tests API** → opens `http://localhost:5000/scalar/v1` (dev) → interactive API docs → tests endpoints directly

## Not yet implemented

- End-user browsing/purchasing flow (depends on Ordering module)
- Admin persona with cross-owner visibility
- Scalar UI in production (blocked intentionally — attack surface)
