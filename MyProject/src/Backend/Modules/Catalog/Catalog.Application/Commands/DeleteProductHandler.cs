using BuildingBlocks.Application;
using Modules.Catalog.Domain;

namespace Modules.Catalog.Application;

public class DeleteProductHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(
        DeleteProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetAsync(command.Id, cancellationToken)
            ?? throw new CatalogNotFoundException(command.Id);

        if (product.OwnerId != command.OwnerId)
        {
            throw new CatalogForbiddenException(product.OwnerId, command.OwnerId);
        }

        repository.Delete(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
