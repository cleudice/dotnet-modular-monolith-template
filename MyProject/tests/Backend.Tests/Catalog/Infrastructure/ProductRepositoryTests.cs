using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain;
using Modules.Catalog.Infrastructure;

namespace Backend.Tests.Catalog.Infrastructure;

public class ProductRepositoryTests : IDisposable
{
    private const string OwnerId = "user-1";
    private const string OtherOwnerId = "user-2";

    private readonly CatalogDbContext _dbContext;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CatalogDbContext(options);
        _repository = new ProductRepository(_dbContext);
    }

    [Fact]
    public async Task AddAsync_Then_GetAsync_ReturnsProduct()
    {
        var product = Product.Create("Test", null, 10m, "SKU-001", 5, OwnerId);
        await _repository.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAsync(product.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Sku.Should().Be("SKU-001");
        result.OwnerId.Should().Be(OwnerId);
    }

    [Fact]
    public async Task GetBySkuAsync_ExistingSku_ReturnsProduct()
    {
        var product = Product.Create("Test", null, 10m, "SKU-FIND", 5, OwnerId);
        await _repository.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetBySkuAsync("SKU-FIND");

        result.Should().NotBeNull();
        result!.Sku.Should().Be("SKU-FIND");
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByOwner_ReturnsOnlyOwnedProducts()
    {
        for (int i = 1; i <= 3; i++)
        {
            var product = Product.Create($"Product {i}", null, 10m, $"SKU-00{i}", i, OwnerId);
            await _repository.AddAsync(product);
        }

        var otherProduct = Product.Create("Other", null, 10m, "SKU-OTHER", 1, OtherOwnerId);
        await _repository.AddAsync(otherProduct);

        await _dbContext.SaveChangesAsync();

        var (items, totalCount) = await _repository.GetPagedAsync(OwnerId, 1, 10);

        totalCount.Should().Be(3);
        items.Should().HaveCount(3);
        items.Should().AllSatisfy(p => p.OwnerId.Should().Be(OwnerId));
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPage()
    {
        for (int i = 1; i <= 5; i++)
        {
            var product = Product.Create($"Product {i}", null, 10m, $"SKU-0{i}", i, OwnerId);
            await _repository.AddAsync(product);
        }

        await _dbContext.SaveChangesAsync();

        var (items, totalCount) = await _repository.GetPagedAsync(OwnerId, 1, 3);

        totalCount.Should().Be(5);
        items.Should().HaveCount(3);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
