using BuildingBlocks.Application;
using Modules.Catalog.Domain;

namespace Modules.Catalog.Application;

public class UpdateProductHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task<ProductDto> HandleAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetAsync(command.Id, cancellationToken)
            ?? throw new CatalogNotFoundException(command.Id);

        if (product.OwnerId != command.OwnerId)
        {
            throw new CatalogForbiddenException(product.OwnerId, command.OwnerId);
        }

        product.Update(command.Name, command.Description, command.Price);

        repository.Update(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }
}
