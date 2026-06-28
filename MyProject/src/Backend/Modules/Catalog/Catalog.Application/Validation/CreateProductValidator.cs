using FluentValidation;
using Modules.Catalog.Domain;

namespace Modules.Catalog.Application;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(ProductName.MaxLength)
            .WithMessage($"Product name must not exceed {ProductName.MaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(Product.MaxDescriptionLength)
            .WithMessage($"Product description must not exceed {Product.MaxDescriptionLength} characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Product price must be greater than zero.")
            .LessThan(Price.MaxDisplayValue)
            .WithMessage($"Product price must be less than {Price.MaxDisplayValue:N0}.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("Product SKU is required.")
            .MaximumLength(Sku.MaxLength)
            .WithMessage($"Product SKU must not exceed {Sku.MaxLength} characters.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must not be negative.");
    }
}
