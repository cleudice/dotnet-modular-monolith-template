using BuildingBlocks.Application;
using Modules.Catalog.Domain;

namespace Modules.Catalog.Application;

public class CreateProductHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<ProductDto> HandleAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetBySkuAsync(command.Sku, cancellationToken);
        if (existing is not null)
        {
            throw new CatalogValidationException($"A product with SKU '{command.Sku}' already exists.");
        }

        var product = Product.Create(
            command.Name,
            command.Description,
            command.Price,
            command.Sku,
            command.StockQuantity,
            command.OwnerId);

        await repository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }
}
