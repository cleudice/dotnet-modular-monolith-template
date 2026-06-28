using BuildingBlocks.Application;
using BuildingBlocks.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Application;
using Modules.Catalog.Domain;

namespace Modules.Catalog.Infrastructure;

public static class CatalogModuleRegistration
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' not found. Ensure appsettings.json contains ConnectionStrings:Default.");

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHostedService<OutboxBackgroundService<CatalogDbContext>>();

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<CatalogDbContext>());

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddSingleton<IDbConnectionFactory>(
            new NpgsqlConnectionFactory(connectionString));

        services.AddScoped<ProductQueries>();

        services.AddScoped<GetProductHandler>();
        services.AddScoped<ListProductsHandler>();
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<DeleteProductHandler>();
        services.AddScoped<UpdateProductStockHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

        return services;
    }
}
