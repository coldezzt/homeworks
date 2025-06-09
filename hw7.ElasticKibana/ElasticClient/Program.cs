using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using ElasticClient;

var client = new ElasticsearchClient(new Uri("http://127.0.0.0:9200/"));

const string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
const string docIndex = "lorem_index";

await FillDatabase();

Console.WriteLine("================================================");

Console.WriteLine("Querying...");
var searchResponse = await client.SearchAsync<Doc>(s => s
    .Indices(docIndex)
    .Query(q => q
        .Match(f => f
            .Field(a => a.Data)
            .Query("search")
        )
    )
);

if (searchResponse.IsValidResponse)
{
    Console.WriteLine("Results:");
    foreach (var doc in searchResponse.Documents)
        Console.WriteLine($"\t{doc.Data}");
}
else Console.WriteLine("Nothing found!");

Console.WriteLine("================================================");

Console.WriteLine("Aggregating...");
var aggregateResponse = await client.SearchAsync<Doc>(docIndex, s => s
    .Query(q => q
        .Match(m => m
            .Field(f => f.Data)
            .Query("search")
        )
    )
    .Aggregations(agg => agg
        .Add("category", 
            t => t.Terms(m => m.Field(x => x.Category.Suffix("keyword")))
        )
    )
);

if (aggregateResponse.IsValidResponse)
{
    var m = aggregateResponse.Aggregations!.GetStringTerms("category")!;
    Console.WriteLine($"\t{m.Buckets.Count}");
}
else Console.WriteLine("Couldn't aggregate!");

Console.WriteLine("================================================");

Console.WriteLine("Updating...");
var updateResponse = await client.UpdateAsync<Doc, Doc>(docIndex, 1, u => u.Doc(new Doc {Data = Guid.NewGuid().ToString()}));
if (updateResponse.IsValidResponse)
{
    Console.WriteLine("Updated!");
    Console.WriteLine("Results:");
    var receivedDoc = await client.GetAsync<Doc>(docIndex, 1);
    if (receivedDoc.IsValidResponse)
        Console.WriteLine($"\t{receivedDoc.Source?.Data}");
}
else Console.WriteLine("Couldn't update!");

Console.WriteLine("================================================");

Console.WriteLine("Deleting...");
var deleteResponse = await client.DeleteAsync(docIndex, 1);
if (deleteResponse.IsValidResponse)
{
    Console.WriteLine("Deleted!");
    Console.WriteLine("Results:");
    var receivedDoc = await client.SearchAsync<Doc>(docIndex, s => s
        .Indices(docIndex)
        .Query(q => q
            .Term(t => t
                .Field(d => d.Id)
                .Value(1)
            )
        )
    );
    if (receivedDoc.IsValidResponse)
        foreach (var doc in receivedDoc.Documents)
            Console.WriteLine($"\t{doc.Data}");
}
else Console.WriteLine("Couldn't delete!");

return;


async Task FillDatabase()
{
    await client.Indices.DeleteAsync(docIndex);
    await client.Indices.CreateAsync(docIndex);
    
    var articles = new[]
    {
        new Doc { Id = 1, Data = "Elasticsearch is a powerful search engine", Category = "search" },
        new Doc { Id = 2, Data = "I love full-text search", Category = "search" },
        new Doc { Id = 3, Data = "Machine learning is amazing", Category = "ml" },
        new Doc { Id = 4, Data = "Text analysis is key in NLP", Category = "ml" },
    };

    foreach (var art in articles)
        _ = await client.IndexAsync(art, x => x.Index(docIndex));
    
    await client.Indices.RefreshAsync(docIndex);
    
    Console.WriteLine("Database filled!");
}


