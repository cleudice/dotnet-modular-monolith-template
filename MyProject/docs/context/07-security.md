# 07 — Security

> Load this when: auth, sensitive data, or security review.

## Authentication

| Subsystem | Mechanism |
|-----------|-----------|
| Host.Api | JWT Bearer (primary) + `X-User-Id` header fallback (dev). JWT disabled if `Jwt:Key` is empty — uses header only. |
| ApiGateway | None (transparent proxy — forwards Authorization and X-User-Id headers) |

**JWT config:** `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key` in `.env`. Symmetric signing key. `GetOwnerId()` in `CatalogEndpoints` prefers JWT `sub` claim, falls back to `X-User-Id` header, defaults to `"anonymous"`.

## Authorization

- **Ownership-based:** Every write operation checks `product.OwnerId == command.OwnerId`. Mismatch → `CatalogForbiddenException` → 403.
- **Read isolation:** `GetProductHandler` returns `null` (→ 404) if OwnerId doesn't match. `ListProductsHandler` filters by OwnerId at query level.
- **No role-based authorization yet.**

## Rate Limiting

Fixed window: 100 requests per minute, queue of 10. Excess → 429 Too Many Requests. Configured in `Program.cs` via `AddRateLimiter`. Applied globally via `UseRateLimiter()` middleware.

## CORS

- **Development:** `AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()` (open for local dev).
- **Production:** Configurable origins via `Cors:AllowedOrigins` in `appsettings.json`. Empty by default — no cross-origin requests allowed until configured.

## Secret locations (location only — never the value)

| Secret | Where | Consumed by |
|--------|-------|-------------|
| `ConnectionStrings:Default` | `.env` (dev) / env var (prod) | `CatalogModuleRegistration.AddCatalogModule()` |
| `Jwt:Key` | `.env` (dev) / env var (prod) | `Program.cs` JWT setup |
| GITHUB_TOKEN | GitHub Actions secrets | `backend-cd.yml` → GHCR login |
| UserSecretsId `642e34a4-...` | `Host.Api.csproj` | `dotnet user-secrets` tool |

No secrets in committed files. `appsettings.json` has empty connection string and empty JWT key.

## Attack surface

- **SQL injection:** Low — all DB access parameterized (EF Core + Dapper `CommandDefinition`).
- **SKU enumeration:** `GetBySkuAsync` internal only — no public endpoint, no enumeration vector.
- **Mass assignment:** Commands are immutable `record` types with explicit properties. OwnerId set server-side from auth context, not request body.
- **No file upload, no XSS surface** (JSON API, no HTML rendering).
- **No CSRF surface** (stateless API, no cookies).
- **Rate limiting** prevents brute-force and DoS.
- **Gateway TLS termination** — Host.Api HTTP-only (not exposed externally).
