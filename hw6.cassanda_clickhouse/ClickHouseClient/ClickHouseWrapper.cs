using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using ClickHouse.Client.Utility;

namespace Test;

public class ClickHouseRepository(string connectionString)
{
    public async Task CreateTableAsync(string tableName, string schema)
    {
        await using var connection = new ClickHouseConnection(connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({schema}) ENGINE = MergeTree() ORDER BY tuple()";
        await cmd.ExecuteNonQueryAsync();
    }
    
    public async Task DropTableAsync(string tableName)
    {
        await using var connection = new ClickHouseConnection(connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
        await cmd.ExecuteNonQueryAsync();
    }
    
    public async Task BulkInsertDataAsync(string tableName, DataTable data)
    {
        await using var connection = new ClickHouseConnection(connectionString);
        await connection.OpenAsync();
        using var copy = new ClickHouseBulkCopy(connection)
        {
            DestinationTableName = tableName,
            BatchSize = 10000
        };
        await copy.InitAsync();
        await copy.WriteToServerAsync(data, CancellationToken.None);
    }
    
    public async Task InsertDataAsync(string tableName, DataTable data)
    {
        if (data.Rows.Count == 0) return;

        await using var connection = new ClickHouseConnection(connectionString);
        await connection.OpenAsync();
        
        foreach (DataRow row in data.Rows)
        {
            var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO {tableName} (id, name, age) VALUES (@id, @name, @age)";
            command.AddParameter("id", row["id"]);
            command.AddParameter("name", row["name"]);
            command.AddParameter("age", row["age"]);

            await command.ExecuteNonQueryAsync();
        }
    }

    
    public async Task DeleteDataAsync(string tableName, List<Condition> conditions, string logicalOperator = "AND")
    {
        if (conditions.Count == 0) throw new ArgumentException("At least one condition must be provided", nameof(conditions));

        await using var connection = new ClickHouseConnection(connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        var whereClauses = conditions.Select((c, i) => 
        {
            var paramName = $"v{i}";
            cmd.AddParameter(paramName, c.Value);
            return $"{c.Column} {c.Operator} @{paramName}";
        });

        cmd.CommandText = $"ALTER TABLE {tableName} DELETE WHERE {string.Join($" {logicalOperator} ", whereClauses)}";

        await cmd.ExecuteNonQueryAsync();
    }
    
    public async Task<List<Dictionary<string, object>>> SearchAsync(string tableName, string column, string op, object value)
    {
        await using var connection = new ClickHouseConnection(connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.AddParameter("v0", value);
        cmd.CommandText = $"SELECT * FROM {tableName} WHERE {column} {op} @v0";
        await using var reader = await cmd.ExecuteReaderAsync();
        return await ReadResultsAsync(reader);
    }
    
    public async Task<List<Dictionary<string, object>>> SearchWithConditionsAsync(string tableName, List<Condition> conditions, string logicalOperator = "AND")
    {
        await using var connection = new ClickHouseConnection(connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        var whereClauses = conditions.Select((c, i) => 
        {
            var paramName = $"v{i}";
            cmd.AddParameter(paramName, c.Value);
            return $"{c.Column} {c.Operator} @{paramName}";
        });
        cmd.CommandText = $"SELECT * FROM {tableName} WHERE {string.Join($" {logicalOperator} ", whereClauses)}";
        await using var reader = await cmd.ExecuteReaderAsync();
        return await ReadResultsAsync(reader);
    }
    
    public async Task UpdateDataAsync(string tableName, Dictionary<string, object> updates, List<Condition> conditions, string logicalOperator = "AND")
    {
        if (updates.Count == 0) throw new ArgumentException("At least one update must be provided", nameof(updates));
        if (conditions.Count == 0) throw new ArgumentException("At least one condition must be provided", nameof(conditions));
        await using var connection = new ClickHouseConnection(connectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        var setClauses = updates.Select((kv, i) =>
        {
            var paramName = $"v{i}";
            cmd.AddParameter(paramName, kv.Value);
            return $"{kv.Key} = @{paramName}";
        });
        var whereClauses = conditions.Select((c, i) =>
        {
            var paramName = $"w{i}";
            cmd.AddParameter(paramName, c.Value);
            return $"{c.Column} {c.Operator} @w{i}";
        });
        cmd.CommandText = $"ALTER TABLE {tableName} UPDATE {string.Join(", ", setClauses)} WHERE {string.Join($" {logicalOperator} ", whereClauses)}";
        await cmd.ExecuteNonQueryAsync();
    }
    
    private async Task<List<Dictionary<string, object>>> ReadResultsAsync(DbDataReader reader)
    {
        var results = new List<Dictionary<string, object>>();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }
            results.Add(row);
        }
        return results;
    }
}