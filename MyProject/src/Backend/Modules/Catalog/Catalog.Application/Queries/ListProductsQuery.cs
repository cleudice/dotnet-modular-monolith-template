namespace Modules.Catalog.Application;

public record ListProductsQuery(string OwnerId, int Page = 1, int PageSize = 20);
