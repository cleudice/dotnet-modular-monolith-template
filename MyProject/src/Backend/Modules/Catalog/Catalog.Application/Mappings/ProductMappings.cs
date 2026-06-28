using Modules.Catalog.Domain;

namespace Modules.Catalog.Application;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Sku,
            product.StockQuantity,
            product.OwnerId);
}
