using Modules.Catalog.Domain;

namespace Modules.Catalog.Application;

public class GetProductHandler(IProductRepository repository)
{
    public async Task<ProductDto?> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetAsync(query.Id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        if (product.OwnerId != query.OwnerId)
        {
            return null;
        }

        return product.ToDto();
    }
}
