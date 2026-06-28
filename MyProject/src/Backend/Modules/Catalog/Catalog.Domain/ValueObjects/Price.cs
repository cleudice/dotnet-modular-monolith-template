using System.Globalization;

namespace Modules.Catalog.Domain;

public readonly record struct Price
{
    public const decimal MaxValue = 999_999_999_999_999.99m;
    public const int MaxDisplayValue = 1_000_000;

    public decimal Value { get; }

    public Price(decimal value)
    {
        if (value <= 0)
        {
            throw new CatalogValidationException("Product price must be greater than zero.");
        }

        if (value > MaxValue)
        {
            throw new CatalogValidationException($"Product price must not exceed {MaxValue:N2}.");
        }

        Value = value;
    }

    public override string ToString() => Value.ToString("F2", CultureInfo.InvariantCulture);

    public static implicit operator decimal(Price price) => price.Value;
}
