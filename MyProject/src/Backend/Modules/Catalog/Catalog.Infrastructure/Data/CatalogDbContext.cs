using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain;
namespace Modules.Catalog.Infrastructure;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : AppDbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(ProductName.MaxLength);

            entity.Property(p => p.Description)
                .HasMaxLength(Product.MaxDescriptionLength);

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(Sku.MaxLength);

            entity.HasIndex(p => p.Sku)
                .IsUnique();

            entity.Property(p => p.StockQuantity)
                .IsRequired();

            entity.Property(p => p.OwnerId)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(p => new { p.OwnerId, p.Id });
        });
    }
}
