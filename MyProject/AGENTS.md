# Project Context — Dotnet Modular Monolith Template

> Lean map. Detail lives in docs/context/ (one file per axis).
> For Claude Code, keep a CLAUDE.md containing only: "Read AGENTS.md".

## What it is

An open-source .NET 10.0 modular monolith template with Clean Architecture, DDD tactical patterns, CQRS (hand-rolled, no MediatR), Minimal APIs, EF Core (writes) + Dapper (reads), FluentValidation, Outbox pattern, YARP gateway, JWT auth, and Serilog. Catalog module included as reference implementation. New modules scaffolded via `./scripts/add-module.sh`.

## Subsystems

| Path | Stack | Role |
|------|-------|------|
| src/Backend/Host.Api | ASP.NET Core 10.0 + Scalar | Composition root — wires DI, registers modules, serves API docs via Scalar |
| src/Backend/ApiGateway | ASP.NET Core 10.0 + YARP 2.3.0 | Reverse proxy — routes `/api/*`, `/openapi/*`, `/scalar/*` to Host.Api, TLS termination |
| src/Backend/BuildingBlocks | .NET 10.0 class libraries | Shared kernel — Entity, ValueObject, AggregateRoot, AppDbContext, OutboxMessage, OutboxBackgroundService<T> |
| src/Backend/Modules/Catalog | .NET 10.0 class libraries | Product catalog — full CRUD with ownership |
| src/Frontend | Node/JS (placeholder) | Empty package.json — frontend stack not yet chosen |
| tests/Backend.Tests | xUnit + FluentAssertions + Testcontainers | 28 tests — unit (InMemory) + integration (real PostgreSQL) |
| .github/workflows | GitHub Actions | backend-ci.yml (build+test), backend-cd.yml (publish to GHCR), frontend-ci.yml (placeholder) |
| .template.config | dotnet new template | template.json — sourceName=MyProject, symbols: EnableJwt, ModuleName |
| scripts/add-module.sh | Bash script | Scaffolds new business module from Catalog pattern |

## Template

```bash
dotnet new install .                    # Install template
dotnet new modular-monolith -n MyApp    # Create project
cd MyApp && ./scripts/add-module.sh Orders  # Add module
```

## Docker

```bash
docker compose up -d                # PostgreSQL + Host.Api + ApiGateway
docker compose --profile dev up -d  # Dev mode with hot reload (future)
```

Ports: Gateway 5000, Host.Api 5100, PostgreSQL 5432.

## Where the context lives

Load only the axis you need from docs/context/: 01 overview · 02 architecture · 03 integrations · 04 data · 05 personas · 06 entry points · 07 security · 08 conventions · 09 maintenance · 10 feature guide.

## Before any change

Read docs/context/09-maintenance.md (gotchas).
