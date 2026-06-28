namespace Modules.Catalog.Domain;

public sealed class CatalogNotFoundException : CatalogDomainException
{
    public long ProductId { get; }

    public CatalogNotFoundException(long productId)
        : base($"Product with Id {productId} was not found.")
    {
        ProductId = productId;
    }
}
