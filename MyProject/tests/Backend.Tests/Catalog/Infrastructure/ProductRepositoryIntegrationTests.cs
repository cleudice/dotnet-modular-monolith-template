using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain;
using Modules.Catalog.Infrastructure;
using Testcontainers.PostgreSql;

namespace Backend.Tests.Catalog.Infrastructure;

public sealed class ProductRepositoryIntegrationTests : IAsyncLifetime, IDisposable
{
    private const string OwnerId = "user-1";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private CatalogDbContext _dbContext = null!;
    private ProductRepository _repository = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _dbContext = new CatalogDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _repository = new ProductRepository(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task AddAsync_WithValidProduct_PersistsToDatabase()
    {
        // Arrange
        var product = Product.Create(
            "Keyboard", "Mechanical", 299.90m, "KB-001", 50, OwnerId);

        // Act
        await _repository.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetAsync(product.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Keyboard");
        retrieved.Sku.Should().Be("KB-001");
    }

    [Fact]
    public async Task GetBySkuAsync_WithExistingSku_ReturnsProduct()
    {
        // Arrange
        var product = Product.Create(
            "Mouse", "Wireless", 149.90m, "MOU-001", 100, OwnerId);
        await _repository.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySkuAsync("MOU-001");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Mouse");
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByOwner()
    {
        // Arrange
        await _repository.AddAsync(Product.Create(
            "A", null, 10m, "SKU-A", 1, OwnerId));
        await _repository.AddAsync(Product.Create(
            "B", null, 20m, "SKU-B", 1, "other-user"));
        await _dbContext.SaveChangesAsync();

        // Act
        var (items, total) = await _repository.GetPagedAsync(OwnerId, 1, 20);

        // Assert
        total.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].OwnerId.Should().Be(OwnerId);
    }

    [Fact]
    public async Task AddAsync_WithDuplicateSku_ThrowsOnRealDatabase()
    {
        // Arrange
        var product1 = Product.Create(
            "Item 1", null, 10m, "DUP-001", 1, OwnerId);
        var product2 = Product.Create(
            "Item 2", null, 20m, "DUP-001", 1, OwnerId);

        await _repository.AddAsync(product1);
        await _dbContext.SaveChangesAsync();

        // Act
        await _repository.AddAsync(product2);

        // Assert — real PostgreSQL enforces unique index
        var act = () => _dbContext.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
