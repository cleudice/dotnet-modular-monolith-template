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
            });

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
            });

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
            });

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
            });

        group.MapDelete("/products/{id:long}",
            async (long id, HttpContext httpContext, DeleteProductHandler handler, CancellationToken ct) =>
            {
                var ownerId = GetOwnerId(httpContext);
                await handler.HandleAsync(new DeleteProductCommand(id, ownerId), ct);
                return Results.NoContent();
            });

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
            });

        return routes;
    }

    private static string GetOwnerId(HttpContext httpContext) =>
        httpContext.Request.Headers[OwnerIdHeader].FirstOrDefault() ?? DefaultOwnerId;
}
