using Modules.Catalog.Domain;

namespace Modules.Catalog.Application;

public class ListProductsHandler(IProductRepository repository)
{
    public async Task<ListProductsResult> HandleAsync(
        ListProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await repository.GetPagedAsync(
            query.OwnerId, query.Page, query.PageSize, cancellationToken);

        var dtos = items.Select(p => p.ToDto()).ToList();

        return new ListProductsResult(
            dtos.AsReadOnly(),
            totalCount,
            query.Page,
            query.PageSize);
    }
}

public record ListProductsResult(
    IReadOnlyList<ProductDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
