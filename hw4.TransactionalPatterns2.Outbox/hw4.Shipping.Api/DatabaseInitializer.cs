using Dapper;
using Npgsql;

namespace Shipping.Api;

internal sealed class DatabaseInitializer(
    NpgsqlDataSource dataSource,
    IConfiguration configuration,
    ILogger<DatabaseInitializer> logger)
{
    public async Task Execute(CancellationToken stoppingToken = default)
    {
        try
        {
            logger.LogInformation("Starting database initialization.");

            await EnsureDatabaseExists();
            await InitializeDatabase();

            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
        }
    }

    private async Task EnsureDatabaseExists()
    {
        var connectionString = configuration.GetConnectionString("Database")!;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres"; // Connect to the default 'postgres' database

        await using var connection = new NpgsqlConnection(builder.ToString());
        await connection.OpenAsync();

        var databaseExists = await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @databaseName)",
            new { databaseName });

        if (!databaseExists)
        {
            logger.LogInformation("Creating database {DatabaseName}", databaseName);
            await connection.ExecuteAsync($"CREATE DATABASE {databaseName}");
        }
    }

    private async Task InitializeDatabase()
    {
        const string sql =
            """
            -- Create shipments table if it doesn't exist
            CREATE TABLE IF NOT EXISTS shipments (
                id UUID PRIMARY KEY,
                order_id UUID NOT NULL,
                status VARCHAR(20) NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL,
                updated_at TIMESTAMP WITH TIME ZONE
            );
            
            -- Create index on order_id for faster lookups
            CREATE INDEX IF NOT EXISTS idx_shipments_order_id ON shipments(order_id);
            """;
        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(sql);
    }
}
