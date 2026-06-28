using FluentAssertions;
using Modules.Catalog.Application;

namespace Backend.Tests.Catalog.Application;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        var command = new CreateProductCommand("Product", null, 10m, "SKU-001", 0, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyName_IsInvalid()
    {
        var command = new CreateProductCommand("", null, 10m, "SKU-001", 0, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithZeroPrice_IsInvalid()
    {
        var command = new CreateProductCommand("Product", null, 0m, "SKU-001", 0, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task Validate_WithEmptySku_IsInvalid()
    {
        var command = new CreateProductCommand("Product", null, 10m, "", 0, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public async Task Validate_WithNegativeStockQuantity_IsInvalid()
    {
        var command = new CreateProductCommand("Product", null, 10m, "SKU-001", -1, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StockQuantity");
    }
}
