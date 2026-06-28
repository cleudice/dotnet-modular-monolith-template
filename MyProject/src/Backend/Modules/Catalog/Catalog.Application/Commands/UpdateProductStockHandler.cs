using BuildingBlocks.Application;
using Modules.Catalog.Domain;

namespace Modules.Catalog.Application;

public class UpdateProductStockHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<ProductDto> HandleAsync(
        UpdateProductStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetAsync(command.ProductId, cancellationToken)
            ?? throw new CatalogNotFoundException(command.ProductId);

        if (product.OwnerId != command.OwnerId)
        {
            throw new CatalogForbiddenException(product.OwnerId, command.OwnerId);
        }

        product.UpdateStock(command.Quantity);

        repository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }
}
