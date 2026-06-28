using System.ComponentModel;

namespace Modules.Catalog.Application;

public record UpdateProductCommand(
    [property: Description("Product identifier (must match route id)")] long Id,
    [property: Description("Updated product name (required, max 200 characters)")] string Name,
    [property: Description("Updated description (optional, max 1000 characters)")] string? Description,
    [property: Description("Updated unit price in BRL (must be positive)")] decimal Price,
    [property: Description("Owner user identifier — set server-side from X-User-Id header")] string OwnerId);
