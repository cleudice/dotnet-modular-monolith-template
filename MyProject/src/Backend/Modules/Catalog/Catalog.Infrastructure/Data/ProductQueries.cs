using BuildingBlocks.Infrastructure;
using Dapper;
using Modules.Catalog.Application;

namespace Modules.Catalog.Infrastructure;

public class ProductQueries(IDbConnectionFactory connectionFactory)
{
    public async Task<ProductDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<ProductDto>(
            new CommandDefinition(
                """
                SELECT Id, Name, Description, Price, Sku, StockQuantity, OwnerId
                FROM catalog."Products"
                WHERE Id = @Id
                """,
                new { Id = id },
                cancellationToken: cancellationToken));
    }

    public async Task<(IReadOnlyList<ProductDto> Items, int TotalCount)> ListAsync(
        string ownerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
        }

        using var connection = connectionFactory.CreateConnection();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """SELECT COUNT(*) FROM catalog."Products" WHERE "OwnerId" = @OwnerId """,
                new { OwnerId = ownerId },
                cancellationToken: cancellationToken));

        var items = await connection.QueryAsync<ProductDto>(
            new CommandDefinition(
                """
                SELECT Id, Name, Description, Price, Sku, StockQuantity, OwnerId
                FROM catalog."Products"
                WHERE "OwnerId" = @OwnerId
                ORDER BY Id
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY
                """,
                new { OwnerId = ownerId, Offset = (page - 1) * pageSize, Limit = pageSize },
                cancellationToken: cancellationToken));

        return (items.ToList().AsReadOnly(), totalCount);
    }
}
