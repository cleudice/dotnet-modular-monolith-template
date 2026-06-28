namespace Modules.Catalog.Domain;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        string ownerId, int page, int pageSize, CancellationToken cancellationToken = default);
    void Update(Product product);
    void Delete(Product product);
}
