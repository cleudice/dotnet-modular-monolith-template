using System.ComponentModel;

namespace Modules.Catalog.Application;

public record CreateProductCommand(
    [property: Description("Product name (required, max 200 characters)")] string Name,
    [property: Description("Optional description (max 1000 characters)")] string? Description,
    [property: Description("Unit price in BRL (must be positive)")] decimal Price,
    [property: Description("Stock Keeping Unit — unique product code (max 50 characters)")] string Sku,
    [property: Description("Initial stock quantity (must not be negative)")] int StockQuantity,
    [property: Description("Owner user identifier — set server-side from X-User-Id header")] string OwnerId);
