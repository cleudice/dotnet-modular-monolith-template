namespace Modules.Catalog.Application;

public record UpdateProductStockCommand(
    long ProductId,
    int Quantity,
    string OwnerId);
