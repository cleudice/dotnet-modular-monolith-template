namespace Modules.Catalog.Domain;

public readonly record struct Sku
{
    public const int MaxLength = 50;

    public string Value { get; }

    public Sku(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new CatalogValidationException("Product SKU is required.");
        }

        if (trimmed.Length > MaxLength)
        {
            throw new CatalogValidationException($"Product SKU must not exceed {MaxLength} characters.");
        }

        Value = trimmed.ToUpperInvariant();
    }

    public override string ToString() => Value;

    public static implicit operator string(Sku sku) => sku.Value;
}
