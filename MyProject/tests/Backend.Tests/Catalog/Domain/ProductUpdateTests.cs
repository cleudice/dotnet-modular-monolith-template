using FluentAssertions;
using Modules.Catalog.Domain;

namespace Backend.Tests.Catalog.Domain;

public class ProductUpdateTests
{
    private const string OwnerId = "user-1";

    [Fact]
    public void Update_WithValidData_UpdatesProperties()
    {
        var product = Product.Create("Original", "Old desc", 10m, "SKU-001", 5, OwnerId);

        product.Update("Updated Name", "New desc", 99.99m);

        product.Name.Should().Be("Updated Name");
        product.Description.Should().Be("New desc");
        product.Price.Should().Be(99.99m);
        product.Sku.Should().Be("SKU-001");
        product.OwnerId.Should().Be(OwnerId);
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsCatalogValidationException()
    {
        var product = Product.Create("Original", null, 10m, "SKU-001", 5, OwnerId);

        var act = () => product.Update("", null, 50m);

        act.Should().Throw<CatalogValidationException>();
    }

    [Fact]
    public void UpdateStock_WithValidQuantity_UpdatesStock()
    {
        var product = Product.Create("Product", null, 10m, "SKU-001", 5, OwnerId);

        product.UpdateStock(25);

        product.StockQuantity.Should().Be(25);
    }

    [Fact]
    public void UpdateStock_WithNegativeQuantity_ThrowsCatalogValidationException()
    {
        var product = Product.Create("Product", null, 10m, "SKU-001", 5, OwnerId);

        var act = () => product.UpdateStock(-1);

        act.Should().Throw<CatalogValidationException>();
    }
}
