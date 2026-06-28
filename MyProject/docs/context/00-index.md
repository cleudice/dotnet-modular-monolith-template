# Project Context — Dotnet Modular Monolith Template

> Reverse-engineered context pack. Each file covers one axis and loads independently.
> Later tasks load only the axis they need.

## Map

| File | Axis | Load when |
|------|------|-----------|
| 01-overview.md | Overview | first contact |
| 02-architecture.md | Architecture | designing or reviewing a solution |
| 03-integrations.md | Integrations | touching a cross-service contract |
| 04-data-flow.md | Data | touching DB / repository / query |
| 05-personas-scenarios.md | Personas | understanding a business requirement |
| 06-entry-points.md | Entry points | locating where a flow begins |
| 07-security.md | Security | auth, sensitive data, security review |
| 08-conventions.md | Conventions | writing or reviewing code |
| 09-maintenance.md | Maintenance | ANY change — read the gotchas |
| 10-feature-guide.md | Feature guide | adding a feature |

## Skipped axes

None. All ten axes apply.

## Domain glossary

| Term | Meaning |
|------|---------|
| Module | A vertical business capability (Catalog, Ordering) with its own Domain/Application/Infrastructure layers |
| BuildingBlocks | Shared kernel — base classes and cross-cutting infrastructure reused by all modules |
| Host.Api | Composition root — the ASP.NET Core host that registers all modules and starts the application |
| ApiGateway | YARP reverse proxy (port 5019) — single entry point that routes `/api/*` to Host.Api |
| Value Object | Immutable `readonly record struct` with self-validation — ProductName, Sku, Price |
| IHasOwner | Interface marking entities that belong to a user (`string OwnerId`) |
| CatalogNotFoundException | Typed domain exception mapped to HTTP 404 by middleware |
| CatalogForbiddenException | Typed domain exception mapped to HTTP 403 when ownership check fails |
| CatalogValidationException | Typed domain exception mapped to HTTP 400 for domain rule violations |
