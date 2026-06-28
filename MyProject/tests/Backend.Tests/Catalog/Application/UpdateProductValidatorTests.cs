using FluentAssertions;
using Modules.Catalog.Application;

namespace Backend.Tests.Catalog.Application;

public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_IsValid()
    {
        var command = new UpdateProductCommand(1, "Updated", "Desc", 50m, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyName_IsInvalid()
    {
        var command = new UpdateProductCommand(1, "", null, 50m, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_WithZeroPrice_IsInvalid()
    {
        var command = new UpdateProductCommand(1, "Name", null, 0m, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task Validate_WithDescriptionTooLong_IsInvalid()
    {
        var command = new UpdateProductCommand(1, "Name", new string('x', 1001), 50m, "user-1");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
