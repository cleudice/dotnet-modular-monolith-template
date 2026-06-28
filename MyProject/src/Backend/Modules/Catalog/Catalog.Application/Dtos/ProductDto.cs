namespace Modules.Catalog.Application;

public record ProductDto(
    long Id,
    string Name,
    string? Description,
    decimal Price,
    string Sku,
    int StockQuantity,
    string OwnerId);
