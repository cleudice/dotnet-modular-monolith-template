namespace Modules.Catalog.Domain;

public readonly record struct ProductName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ProductName(string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new CatalogValidationException("Product name is required.");
        }

        if (trimmed.Length > MaxLength)
        {
            throw new CatalogValidationException($"Product name must not exceed {MaxLength} characters.");
        }

        Value = trimmed;
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductName name) => name.Value;
}
