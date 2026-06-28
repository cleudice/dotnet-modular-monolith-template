using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application;
using Modules.Catalog.Domain;

namespace Modules.Catalog.Infrastructure;

public static class CatalogEndpoints
{
    private const string OwnerIdHeader = "X-User-Id";
    private const string DefaultOwnerId = "anonymous";

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/catalog").WithTags("Catalog");

        group.MapGet("/products/{id:long}",
            async (long id, HttpContext httpContext, GetProductHandler handler, CancellationToken ct) =>
            {
                var ownerId = GetOwnerId(httpContext);
                var product = await handler.HandleAsync(new GetProductQuery(id, ownerId), ct);
                return product is null ? Results.NotFound() : Results.Ok(product);
            })
            .WithSummary("Get a product by ID")
            .WithDescription("Returns the product if it belongs to the authenticated user. Returns 404 if not found or not owned by the caller.");

        group.MapGet("/products",
            async (int? page, int? pageSize, HttpContext httpContext, ListProductsHandler handler, CancellationToken ct) =>
            {
                var resolvedPage = page ?? 1;
                var resolvedPageSize = pageSize ?? 20;

                if (resolvedPage < 1)
                {
                    return Results.BadRequest("Page must be greater than zero.");
                }

                if (resolvedPageSize < 1)
                {
                    return Results.BadRequest("Page size must be greater than zero.");
                }

                var ownerId = GetOwnerId(httpContext);
                var query = new ListProductsQuery(ownerId, resolvedPage, resolvedPageSize);
                var result = await handler.HandleAsync(query, ct);
                return Results.Ok(result);
            })
            .WithSummary("List products")
            .WithDescription("Returns a paginated list of products belonging to the authenticated user. Supports page and pageSize query parameters (defaults: 1 and 20).");

        group.MapPost("/products",
            async (CreateProductCommand command,
                   CreateProductHandler handler,
                   IValidator<CreateProductCommand> validator,
                   HttpContext httpContext,
                   CancellationToken ct) =>
            {
                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var commandWithOwner = command with { OwnerId = GetOwnerId(httpContext) };

                try
                {
                    var product = await handler.HandleAsync(commandWithOwner, ct);
                    return Results.Created($"/api/catalog/products/{product.Id}", product);
                }
                catch (CatalogValidationException ex)
                    when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Conflict(ex.Message);
                }
                catch (DbUpdateException ex)
                    when (ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Results.Conflict($"A product with SKU '{command.Sku}' already exists.");
                }
            })
            .WithSummary("Create a product")
            .WithDescription("Creates a new product for the authenticated user. Validates all fields and checks SKU uniqueness. Returns 201 with the created product, 400 on validation error, or 409 if SKU already exists.");

        group.MapPut("/products/{id:long}",
            async (long id,
                   UpdateProductCommand command,
                   UpdateProductHandler handler,
                   IValidator<UpdateProductCommand> validator,
                   HttpContext httpContext,
                   CancellationToken ct) =>
            {
                if (id != command.Id)
                {
                    return Results.BadRequest("Route id does not match command id.");
                }

                var validationResult = await validator.ValidateAsync(command, ct);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var commandWithOwner = command with { OwnerId = GetOwnerId(httpContext) };
                var product = await handler.HandleAsync(commandWithOwner, ct);
                return Results.Ok(product);
            })
            .WithSummary("Update a product")
            .WithDescription("Updates name, description, and price of a product. Ownership check enforced — returns 403 if caller is not the owner, 404 if not found, 400 on validation error.");

        group.MapDelete("/products/{id:long}",
            async (long id, HttpContext httpContext, DeleteProductHandler handler, CancellationToken ct) =>
            {
                var ownerId = GetOwnerId(httpContext);
                await handler.HandleAsync(new DeleteProductCommand(id, ownerId), ct);
                return Results.NoContent();
            })
            .WithSummary("Delete a product")
            .WithDescription("Deletes the product if the authenticated user is the owner. Returns 204 on success, 403 if not the owner, 404 if not found.");

        group.MapPatch("/products/{id:long}/stock",
            async (long id, int quantity, HttpContext httpContext, UpdateProductStockHandler handler, CancellationToken ct) =>
            {
                if (quantity < 0)
                {
                    return Results.BadRequest("Quantity must not be negative.");
                }

                var ownerId = GetOwnerId(httpContext);
                var product = await handler.HandleAsync(
                    new UpdateProductStockCommand(id, quantity, ownerId), ct);

                return Results.Ok(product);
            })
            .WithSummary("Update product stock")
            .WithDescription("Sets the stock quantity of a product. Ownership check enforced. Quantity must not be negative. Returns 403 if not the owner, 404 if not found.");

        return routes;
    }

    private static string GetOwnerId(HttpContext httpContext)
    {
        // Prefer JWT sub claim when authenticated
        var subClaim = httpContext.User.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(subClaim))
        {
            return subClaim;
        }

        // Fallback to X-User-Id header (dev convenience)
        return httpContext.Request.Headers[OwnerIdHeader].FirstOrDefault() ?? DefaultOwnerId;
    }
}
