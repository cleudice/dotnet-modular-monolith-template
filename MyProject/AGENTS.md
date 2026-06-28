# Project Context — Dotnet Modular Monolith Template

> Lean map. Detail lives in docs/context/ (one file per axis).
> For Claude Code, keep a CLAUDE.md containing only: "Read AGENTS.md".

## What it is

An open-source .NET 10.0 modular monolith template with Clean Architecture, DDD tactical patterns, CQRS (hand-rolled, no MediatR), Minimal APIs, EF Core (writes) + Dapper (reads), FluentValidation, and manual mapping. Two business modules: Catalog (fully implemented) and Ordering (placeholder).

## Subsystems

| Path | Stack | Role |
|------|-------|------|
| src/Backend/Host.Api | ASP.NET Core 10.0 | Composition root — wires DI, registers modules, starts the app |
| src/Backend/ApiGateway | ASP.NET Core 10.0 + YARP 2.3.0 | Reverse proxy — routes `/api/*` to Host.Api, TLS termination, no module references |
| src/Backend/BuildingBlocks | .NET 10.0 class libraries | Shared kernel — Entity, ValueObject, AggregateRoot, AppDbContext, IUnitOfWork |
| src/Backend/Modules/Catalog | .NET 10.0 class libraries | Product catalog — full CRUD with ownership |
| src/Backend/Modules/Ordering | .NET 10.0 class libraries | Order management (placeholder — Class1.cs only) |
| src/Frontend | Node/JS (placeholder) | Empty package.json — frontend stack not yet chosen |
| tests/Backend.Tests | xUnit + FluentAssertions | 24 tests covering Catalog domain, validators, and repository |

## Where the context lives

Load only the axis you need from docs/context/: 01 overview · 02 architecture · 03 integrations · 04 data · 05 personas · 06 entry points · 07 security · 08 conventions · 09 maintenance · 10 feature guide.

## Before any change

Read docs/context/09-maintenance.md (gotchas).
