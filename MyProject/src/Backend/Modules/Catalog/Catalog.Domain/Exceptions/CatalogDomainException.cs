using BuildingBlocks.Domain;

namespace Modules.Catalog.Domain;

public class CatalogDomainException : DomainException
{
    public CatalogDomainException(string message)
        : base(message)
    {
    }

    public CatalogDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
