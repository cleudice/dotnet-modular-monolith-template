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
| Module | A vertical business capability (Catalog, etc.) with its own Domain/Application/Infrastructure layers. Scaffold via `./scripts/add-module.sh`. |
| BuildingBlocks | Shared kernel — base classes, OutboxMessage, OutboxBackgroundService<T>, DomainEventDispatcher |
| Host.Api | Composition root — ASP.NET Core host, wires DI, Serilog, JWT, rate limiting, health checks |
| ApiGateway | YARP reverse proxy (port 5000) — single entry point, TLS termination, no module references |
| Value Object | Immutable `readonly record struct` with self-validation — ProductName, Sku, Price |
| IHasOwner | Interface marking entities that belong to a user (`string OwnerId`) |
| OutboxMessage | Serialized domain event stored in per-module table, processed by background service |
| OutboxBackgroundService<T> | Generic BackgroundService that polls a module's OutboxMessages, dispatches to handlers |
| IDomainEventHandler<T> | Interface for reacting to domain events (no MediatR) |
| CatalogNotFoundException | Typed domain exception mapped to HTTP 404 |
| CatalogForbiddenException | Typed domain exception mapped to HTTP 403 |
| CatalogValidationException | Typed domain exception mapped to HTTP 400 |
