using BuildingBlocks.Domain;

namespace Modules.Catalog.Domain;

public class Product : AggregateRoot, IHasOwner
{
    public const int MaxDescriptionLength = 1000;

    private Product() { } // EF Core materialization

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public int StockQuantity { get; private set; }
    public string OwnerId { get; private set; } = string.Empty;

    public static Product Create(
        string name,
        string? description,
        decimal price,
        string sku,
        int stockQuantity,
        string ownerId)
    {
        var productName = new ProductName(name);
        var productSku = new Sku(sku);
        var productPrice = new Price(price);
        var trimmedDescription = description?.Trim();
        var trimmedOwnerId = ownerId?.Trim() ?? string.Empty;

        if (trimmedDescription?.Length > MaxDescriptionLength)
        {
            throw new CatalogValidationException("Product description must not exceed 1000 characters.");
        }

        if (stockQuantity < 0)
        {
            throw new CatalogValidationException("Stock quantity must not be negative.");
        }

        if (string.IsNullOrWhiteSpace(trimmedOwnerId))
        {
            throw new CatalogValidationException("Product owner is required.");
        }

        return new Product
        {
            Name = productName.Value,
            Description = trimmedDescription,
            Price = productPrice.Value,
            Sku = productSku.Value,
            StockQuantity = stockQuantity,
            OwnerId = trimmedOwnerId
        };
    }

    public void Update(string name, string? description, decimal price)
    {
        var productName = new ProductName(name);
        var productPrice = new Price(price);
        var trimmedDescription = description?.Trim();

        if (trimmedDescription?.Length > MaxDescriptionLength)
        {
            throw new CatalogValidationException("Product description must not exceed 1000 characters.");
        }

        Name = productName.Value;
        Description = trimmedDescription;
        Price = productPrice.Value;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new CatalogValidationException("Stock quantity must not be negative.");
        }

        StockQuantity = quantity;
    }
}
