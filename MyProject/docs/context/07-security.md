# 07 — Security

> Load this when: auth, sensitive data, or security review.

## Authentication (per subsystem)

| Subsystem | Mechanism |
|-----------|-----------|
| Host.Api | `X-User-Id` header (temporary placeholder). No JWT/OAuth/OIDC yet. |
| ApiGateway | None (boilerplate only) |

**Future:** Replace `X-User-Id` header with JWT Bearer token. The `GetOwnerId()` helper in `CatalogEndpoints` is a single extraction point — replace it with `httpContext.User.FindFirst("sub")?.Value` when auth is added.

## Authorization

- **Ownership-based:** Every write operation (Update, Delete, UpdateStock) checks `product.OwnerId == command.OwnerId`. Mismatch → `CatalogForbiddenException` → 403.
- **Read isolation:** `GetProductHandler` returns `null` (→ 404) if OwnerId doesn't match, avoiding information leakage about other users' products. `ListProductsHandler` filters by OwnerId at the query level.
- **No role-based authorization yet.** Admin cross-owner visibility is not implemented.

## Secret locations (location only — never the value)

| Secret | Where | Consumed by |
|--------|-------|-------------|
| `ConnectionStrings:Default` | `dotnet user-secrets` (dev) / env var (prod) | `CatalogModuleRegistration.AddCatalogModule()` |
| UserSecretsId | `Host.Api.csproj` (`642e34a4-...`) | `dotnet user-secrets` tool |

No secrets in committed files. `appsettings.json` has an empty connection string placeholder.

## Attack surface

- **SQL injection:** Low risk. All database access is parameterized — EF Core generates parameterized SQL; Dapper uses `CommandDefinition` with anonymous parameters (never string concatenation).
- **SKU enumeration:** `GetBySkuAsync` is used internally but not exposed as a public endpoint — no SKU enumeration vector.
- **Mass assignment:** Commands are immutable `record` types with explicit properties. Endpoints construct commands from request body + extracted OwnerId (not from raw request).
- **No file upload, no XSS surface** (JSON API, no HTML rendering).
- **No CSRF surface** (stateless API, no cookies).
