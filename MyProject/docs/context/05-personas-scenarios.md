# 05 — Personas & Scenarios

> Load this when: understanding a business requirement.

## Personas

| Persona | Who | What they do |
|---------|-----|--------------|
| Developer | User of the template | Uses this project as a starting point for their own modular monolith. Reads the Catalog module as a reference, copies the pattern for new modules (Ordering, etc.). |
| Product Owner | Owner of products in Catalog | Creates, updates, deletes their own products. Only sees and manages their own catalog. |

## Key journeys

1. **Developer onboards** → clones repo → runs `dotnet user-secrets set` for connection string → runs `dotnet ef database update` → `dotnet run` → sees Catalog API at `/api/catalog/products`
2. **Developer adds a module** → copies Catalog module structure → creates new Domain/Application/Infrastructure projects → adds `AddXxxModule()` + `MapXxxEndpoints()` calls in `Program.cs`
3. **Product Owner manages catalog** → sends `X-User-Id` header → POST creates product → GET lists own products → PUT updates → DELETE removes → PATCH adjusts stock
4. **Cross-owner isolation** → Owner A's products are invisible to Owner B. Attempting to update another owner's product returns 403 (or 404 in GET to avoid leaking existence).

## Not yet implemented

- End-user browsing/purchasing flow (depends on Ordering module)
- Admin persona with cross-owner visibility
- Authentication via JWT/OAuth (currently uses `X-User-Id` header placeholder)
