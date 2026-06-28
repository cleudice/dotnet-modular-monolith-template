namespace Modules.Catalog.Domain;

public sealed class CatalogValidationException : CatalogDomainException
{
    public CatalogValidationException(string message)
        : base(message)
    {
    }

    public CatalogValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
