namespace Modules.Catalog.Domain;

public sealed class CatalogForbiddenException : CatalogDomainException
{
    public string OwnerId { get; }
    public string RequestedOwnerId { get; }

    public CatalogForbiddenException(string ownerId, string requestedOwnerId)
        : base($"Access denied. Resource is owned by '{ownerId}', request claims '{requestedOwnerId}'.")
    {
        OwnerId = ownerId;
        RequestedOwnerId = requestedOwnerId;
    }
}
