using Dapper;
using Npgsql;

namespace Orders.Api;

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
        string connectionString = configuration.GetConnectionString("Database")!;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        string? databaseName = builder.Database;
        builder.Database = "postgres"; // Connect to the default 'postgres' database

        using var connection = new NpgsqlConnection(builder.ToString());
        await connection.OpenAsync();

        bool databaseExists = await connection.ExecuteScalarAsync<bool>(
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
            -- Create orders table if it doesn't exist
            CREATE TABLE IF NOT EXISTS orders (
                id UUID PRIMARY KEY,
                customer_name VARCHAR(255) NOT NULL,
                product_name VARCHAR(255) NOT NULL,
                quantity INTEGER NOT NULL,
                total_price DECIMAL(18, 2) NOT NULL,
                order_date TIMESTAMP WITH TIME ZONE NOT NULL
            );
            
            CREATE TABLE IF NOT EXISTS outbox_messages (
                id UUID PRIMARY KEY,
                type VARCHAR(255) NOT NULL,
                content JSONB NOT NULL,
                occurred_on_utc TIMESTAMP WITH TIME ZONE NOT NULL,
                processed_on_utc TIMESTAMP WITH TIME ZONE NULL,
                error TEXT NULL
            );
            """;
        using NpgsqlConnection connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(sql);
    }
}
