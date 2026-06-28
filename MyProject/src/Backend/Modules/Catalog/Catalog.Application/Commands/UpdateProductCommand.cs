namespace Modules.Catalog.Application;

public record UpdateProductCommand(
    long Id,
    string Name,
    string? Description,
    decimal Price,
    string OwnerId);
