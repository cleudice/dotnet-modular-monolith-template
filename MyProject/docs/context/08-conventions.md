# 08 — Conventions

> Load this when: writing or reviewing code.

## Language & style

- **Code:** C# 13 / .NET 10.0. English identifiers and comments.
- **Prose:** Portuguese (matching user language).
- **Commits:** [not yet established — repository has no commits].

## Formatters & linters

| Tool | Config | Scope |
|------|--------|-------|
| `.editorconfig` | `MyProject/.editorconfig` | `csharp_prefer_braces = true:warning`, file-scoped namespaces, prefer `is null`, 4-space indent, LF line endings, UTF-8 |
| `Directory.Build.props` | `MyProject/Directory.Build.props` | `EnforceCodeStyleInBuild=true`, `AnalysisLevel=latest-recommended`, NRT enabled, ImplicitUsings enabled |
| `global.json` | `MyProject/global.json` | SDK `10.0.108`, `rollForward: latestMinor` |
| `Directory.Packages.props` | `MyProject/Directory.Packages.props` | Central Package Management — versions defined once |

Run `dotnet format style` before committing (note: .NET 10.0 `dotnet format` has a known crash when writing changes — build with `EnforceCodeStyleInBuild` to see warnings instead).

## Naming & folder organization

| Convention | Example |
|------------|---------|
| Commands: `{Action}ProductCommand` | `CreateProductCommand`, `UpdateProductStockCommand` |
| Queries: `{Action}ProductQuery` | `GetProductQuery`, `ListProductsQuery` |
| Handlers: `{Action}ProductHandler` | `CreateProductHandler`, `GetProductHandler` |
| Validators: `{Action}ProductValidator` | `CreateProductValidator`, `UpdateProductValidator` |
| Endpoints: `Map{Module}Endpoints` | `MapCatalogEndpoints` |
| Module registration: `Add{Module}Module` | `AddCatalogModule` |
| Exceptions: `{Module}{Kind}Exception` | `CatalogValidationException`, `CatalogNotFoundException` |
| Test methods: `Method_Scenario_ExpectedOutcome` | `Create_WithEmptyName_ThrowsCatalogValidationException` |
| Async methods: end in `Async` | `HandleAsync`, `GetAsync`, `AddAsync` |
| Folder per concept | `Entities/`, `ValueObjects/`, `Commands/`, `Queries/`, `Data/`, `Endpoints/` |

## Required patterns

1. **New module** follows Catalog structure exactly: Domain/Application/Infrastructure projects, `AddXxxModule(IConfiguration)` extension, `MapXxxEndpoints(IEndpointRouteBuilder)` extension
2. **Commands and queries** are immutable `record` types
3. **Handlers** use primary constructors for DI, expose `HandleAsync` method
4. **Domain exceptions** extend `CatalogDomainException` (which extends `DomainException`)
5. **Value objects** use `readonly record struct` with `implicit operator` for backward compatibility
6. **FluentValidation** at boundary + domain invariants inside entities/value objects
7. **No MediatR** — handlers are injected directly into Minimal API endpoints
8. **No AutoMapper/Mapster** — use extension methods (`product.ToDto()`)
9. **CancellationToken** propagated through all async chains
10. **ConfigureAwait(false)** in library code (BuildingBlocks projects)

## Known prohibitions

- No secrets in committed config files — use `dotnet user-secrets` for development
- No `async void` — always `async Task`
- No `.Result` or `.Wait()` on tasks — await all the way up
- No empty catch blocks — log and rethrow (`throw;` not `throw ex;`)
- No magic numbers — use named constants from value objects or entity classes
