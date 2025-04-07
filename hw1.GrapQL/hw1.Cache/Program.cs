using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();
app.MapGraphQL();
app.RunWithGraphQLCommands(args);

public class Query
{
    public string? GetCacheValue([Service] IMemoryCache cache, string key) => cache.Get<string>(key);
}

public class Mutation
{
    public string AddValue([Service] IMemoryCache cache, string value)
    {
        var hash = value.GetHashCode();
        cache.Set(hash.ToString(), value);
        return hash.ToString();
    }
}