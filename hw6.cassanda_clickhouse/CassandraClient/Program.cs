using System.Data;
using System.Diagnostics;
using Test;

namespace CassandraClient;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        const string clickHouseConnectionString = "Host=localhost;Port=8123;Username=admin;Password=secret;Database=mydb";
        const string cassandraConnectionString = "ContactPoints=localhost;Port=9042;Keyspace=mydb";

        var clickHouseRepo = new ClickHouseRepository(clickHouseConnectionString);
        var cassandraRepo = new CassandraRepository(cassandraConnectionString);

        const string tableName = "users";
        const string clickHouseSchema = "id Int32, name String, age Int32";
        const string cassandraSchema = "id int, name text, age int, PRIMARY KEY (id)";
        
        await clickHouseRepo.DropTableAsync(tableName);
        await cassandraRepo.DropTableAsync(tableName);
        await clickHouseRepo.CreateTableAsync(tableName, clickHouseSchema);
        await cassandraRepo.CreateTableAsync(tableName, cassandraSchema);
        
        var testData = GenerateTestData(2000);
        var sw = new Stopwatch();
        
        sw.Start();
        await clickHouseRepo.BulkInsertDataAsync(tableName, testData);
        sw.Stop();
        Console.WriteLine($"ClickHouse bulk insertion time: {sw.ElapsedMilliseconds} ms");
        
        sw.Start();
        await clickHouseRepo.InsertDataAsync(tableName, testData);
        sw.Stop();
        Console.WriteLine($"ClickHouse insertion time: {sw.ElapsedMilliseconds} ms");

        sw.Restart();
        await cassandraRepo.InsertDataAsync(tableName, testData);
        sw.Stop();
        Console.WriteLine($"Cassandra insertion time: {sw.ElapsedMilliseconds} ms");
        
        var searchConditions = new List<Condition> { new() { Column = "age", Operator = ">", Value = 30 } };

        sw.Restart();
        var clickHouseResults = await clickHouseRepo.SearchWithConditionsAsync(tableName, searchConditions);
        sw.Stop();
        Console.WriteLine($"ClickHouse search time: {sw.ElapsedMilliseconds} ms, found {clickHouseResults.Count} records");

        sw.Restart();
        var cassandraResults = await cassandraRepo.SearchWithConditionsAsync(tableName, searchConditions);
        sw.Stop();
        Console.WriteLine($"Cassandra search time: {sw.ElapsedMilliseconds} ms, found {cassandraResults.Count} records");
        
        var deleteConditions = new List<Test.Condition> { new Test.Condition { Column = "name", Operator = "=", Value = "User100" } };

        sw.Restart();
        await clickHouseRepo.DeleteDataAsync(tableName, deleteConditions);
        sw.Stop();
        Console.WriteLine($"ClickHouse deletion time: {sw.ElapsedMilliseconds} ms");

        sw.Restart();
        await cassandraRepo.DeleteDataAsync(tableName, deleteConditions);
        sw.Stop();
        Console.WriteLine($"Cassandra deletion time: {sw.ElapsedMilliseconds} ms");
        
        var updates = new Dictionary<string, object> { { "age", 99 } };
        var updateConditions = new List<Condition> { new() { Column = "name", Operator = "=", Value = "User200" } };

        sw.Restart();
        await clickHouseRepo.UpdateDataAsync(tableName, updates, updateConditions);
        sw.Stop();
        Console.WriteLine($"ClickHouse update time: {sw.ElapsedMilliseconds} ms");

        sw.Restart();
        await cassandraRepo.UpdateDataAsync(tableName, updates, updateConditions);
        sw.Stop();
        Console.WriteLine($"Cassandra update time: {sw.ElapsedMilliseconds} ms");
    }

    private static DataTable GenerateTestData(int count)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("id", typeof(int));
        dataTable.Columns.Add("name", typeof(string));
        dataTable.Columns.Add("age", typeof(int));

        for (var i = 0; i < count; i++)
        {
            dataTable.Rows.Add(i, $"User{i}", 20 + i % 50);
        }

        return dataTable;
    }
}