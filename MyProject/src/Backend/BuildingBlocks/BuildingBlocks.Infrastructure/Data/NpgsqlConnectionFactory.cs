using System.Data.Common;
using Npgsql;

namespace BuildingBlocks.Infrastructure;

public sealed class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public DbConnection CreateConnection() =>
        new NpgsqlConnection(connectionString);
}
