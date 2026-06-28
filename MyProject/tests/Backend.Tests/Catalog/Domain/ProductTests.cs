using FluentAssertions;
using Modules.Catalog.Domain;

namespace Backend.Tests.Catalog.Domain;

public class ProductTests
{
    private const string OwnerId = "user-1";

    [Fact]
    public void Create_WithValidData_ReturnsProduct()
    {
        var product = Product.Create("Test Product", "A description", 99.99m, "SKU-001", 10, OwnerId);

        product.Name.Should().Be("Test Product");
        product.Description.Should().Be("A description");
        product.Price.Should().Be(99.99m);
        product.Sku.Should().Be("SKU-001");
        product.StockQuantity.Should().Be(10);
        product.OwnerId.Should().Be(OwnerId);
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsCatalogValidationException()
    {
        var act = () => Product.Create("", null, 10m, "SKU-001", 0, OwnerId);

        act.Should().Throw<CatalogValidationException>()
           .Which.Message.Should().Contain("name");
    }

    [Fact]
    public void Create_WithWhitespaceName_ThrowsCatalogValidationException()
    {
        var act = () => Product.Create("   ", null, 10m, "SKU-001", 0, OwnerId);

        act.Should().Throw<CatalogValidationException>()
           .Which.Message.Should().Contain("name");
    }

    [Fact]
    public void Create_WithZeroPrice_ThrowsCatalogValidationException()
    {
        var act = () => Product.Create("Product", null, 0m, "SKU-001", 0, OwnerId);

        act.Should().Throw<CatalogValidationException>()
           .Which.Message.Should().Contain("price");
    }

    [Fact]
    public void Create_WithEmptySku_ThrowsCatalogValidationException()
    {
        var act = () => Product.Create("Product", null, 10m, "", 0, OwnerId);

        act.Should().Throw<CatalogValidationException>()
           .Which.Message.Should().Contain("SKU");
    }

    [Fact]
    public void Create_WithNegativeStockQuantity_ThrowsCatalogValidationException()
    {
        var act = () => Product.Create("Product", null, 10m, "SKU-001", -1, OwnerId);

        act.Should().Throw<CatalogValidationException>()
           .Which.Message.Should().Contain("Stock");
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ThrowsCatalogValidationException()
    {
        var act = () => Product.Create("Product", null, 10m, "SKU-001", 0, "");

        act.Should().Throw<CatalogValidationException>()
           .Which.Message.Should().Contain("owner");
    }
}
