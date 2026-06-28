namespace Modules.Catalog.Application;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    string Sku,
    int StockQuantity,
    string OwnerId);
