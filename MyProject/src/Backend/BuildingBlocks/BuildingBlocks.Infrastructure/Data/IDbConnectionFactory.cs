using System.Data.Common;

namespace BuildingBlocks.Infrastructure;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
