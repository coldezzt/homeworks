using System.Data;
using Test;

const string connectionString = "Host=localhost;Port=8123;Username=admin;Password=secret;Database=mydb";
var repo = new ClickHouseRepository(connectionString);

await repo.CreateTableAsync("users", "id UUID, name String, age Int32");

var dataTable = new DataTable();
dataTable.Columns.Add("id", typeof(Guid));
dataTable.Columns.Add("name", typeof(string));
dataTable.Columns.Add("age", typeof(int));
dataTable.Rows.Add(Guid.NewGuid(), "Bob", 27);
await repo.InsertDataAsync("users", dataTable);
Console.WriteLine("Done inserting records.\n");

var results = await repo.SearchAsync("users", "age", ">", 25);
Console.WriteLine($"{results.Count} records retrieved (users where age > 25)\n");
foreach (var keyValuePair in results.SelectMany(result => result))
{
    Console.WriteLine($"{keyValuePair.Key}: {keyValuePair.Value}");
}
Console.WriteLine();
var conditions = new List<Condition>
{
    new() { Column = "age", Operator = ">", Value = 20 },
    new() { Column = "name", Operator = "=", Value = "Alice" }
};
var resultsAnd = await repo.SearchWithConditionsAsync("users", conditions, "AND");
Console.WriteLine($"{resultsAnd.Count} records retrieved (users where age > 20 and name = Alice)\n");
foreach (var keyValuePair in resultsAnd.SelectMany(result => result))
{
    Console.WriteLine($"{keyValuePair.Key}: {keyValuePair.Value}");
}

await repo.DeleteDataAsync("users", [new Condition { Column = "name", Operator = "=", Value = "Alice" }]);