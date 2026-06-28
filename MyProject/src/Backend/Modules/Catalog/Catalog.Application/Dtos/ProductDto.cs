using System.ComponentModel;

namespace Modules.Catalog.Application;

public record ProductDto(
    [property: Description("Unique identifier of the product")] long Id,
    [property: Description("Product name (max 200 characters)")] string Name,
    [property: Description("Optional description (max 1000 characters)")] string? Description,
    [property: Description("Unit price in BRL")] decimal Price,
    [property: Description("Stock Keeping Unit — unique product code")] string Sku,
    [property: Description("Available quantity in stock")] int StockQuantity,
    [property: Description("Owner user identifier")] string OwnerId);
