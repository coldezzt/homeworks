using System.Data;
using Cassandra;
using Test;

namespace CassandraClient;

public class CassandraRepository
{
    private readonly ISession _session;

    public CassandraRepository(string connectionString)
    {
        var parts = connectionString.Split(';').ToDictionary(p => p.Split('=')[0], p => p.Split('=')[1]);
        var contactPoints = parts["ContactPoints"].Split(',');
        var port = int.Parse(parts["Port"]);
        var keyspace = parts["Keyspace"];

        var cluster = Cluster.Builder()
            .AddContactPoints(contactPoints)
            .WithPort(port)
            .Build();
        _session = cluster.Connect(keyspace);
    }

    public async Task CreateTableAsync(string tableName, string schema)
    {
        var cql = $"CREATE TABLE IF NOT EXISTS {tableName} ({schema})";
        await _session.ExecuteAsync(new SimpleStatement(cql));
    }

    public async Task DropTableAsync(string tableName)
    {
        var cql = $"DROP TABLE IF EXISTS {tableName}";
        await _session.ExecuteAsync(new SimpleStatement(cql));
    }

    public async Task InsertDataAsync(string tableName, DataTable data)
    {
        var cql = $"INSERT INTO {tableName} (id, name, age) VALUES (?, ?, ?)";
        var prepared = await _session.PrepareAsync(cql);
        var tasks = new List<Task>();

        foreach (DataRow row in data.Rows)
        {
            var bound = prepared.Bind(row["id"], row["name"], row["age"]);
            tasks.Add(_session.ExecuteAsync(bound));
        }

        await Task.WhenAll(tasks);
    }

    public async Task<List<Dictionary<string, object>>> SearchWithConditionsAsync(string tableName, List<Condition> conditions, string logicalOperator = "AND")
    {
        var whereClauses = conditions.Select((c, i) => $"{c.Column} {c.Operator} ?");
        var cql = $"SELECT * FROM {tableName} WHERE {string.Join($" {logicalOperator} ", whereClauses)} ALLOW FILTERING";
        var values = conditions.Select(c => c.Value).ToArray();
        var statement = new SimpleStatement(cql, values);
        var result = await _session.ExecuteAsync(statement);
        return result.Select(row => new Dictionary<string, object>
        {
            ["id"] = row.GetValue<int>("id"),
            ["name"] = row.GetValue<string>("name"),
            ["age"] = row.GetValue<int>("age")
        }).ToList();
    }

    public async Task DeleteDataAsync(string tableName, List<Condition> conditions, string logicalOperator = "AND")
    {
        var results = await SearchWithConditionsAsync(tableName, conditions, logicalOperator);
        var idsToDelete = results.Select(r => r["id"]).ToList();
        var deleteCql = $"DELETE FROM {tableName} WHERE id = ?";
        var tasks = idsToDelete.Select(id => _session.ExecuteAsync(new SimpleStatement(deleteCql, id))).ToList();
        await Task.WhenAll(tasks);
    }

    public async Task UpdateDataAsync(string tableName, Dictionary<string, object> updates, List<Condition> conditions, string logicalOperator = "AND")
    {
        var results = await SearchWithConditionsAsync(tableName, conditions, logicalOperator);
        var idsToUpdate = results.Select(r => r["id"]).ToList();
        var setClauses = string.Join(", ", updates.Select(kv => $"{kv.Key} = {kv.Value}"));
        var updateCql = $"UPDATE {tableName} SET {setClauses} WHERE id = ?";
        var tasks = idsToUpdate.Select(id => _session.ExecuteAsync(new SimpleStatement(updateCql, id))).ToList();
        await Task.WhenAll(tasks);
    }
}