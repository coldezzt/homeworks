using hw1.Query;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt 
    => opt.UseInMemoryDatabase("PersonDB"));
    
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddProjections()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();
app.MapGraphQL();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.People.AddRange(
        new("John", 21),
        new("Jane", 23),
        new("Jack", 10),
        new("Jerry", 25),
        new("Mary", 40),
        new("Mary", 56),
        new("Mary", 32)
    );
    context.SaveChanges();
}


app.RunWithGraphQLCommands(args);

public class Query
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Person> GetPeople([Service] AppDbContext ctxt) => ctxt.People;
}

public class Person(string name, int age)
{
    public long Id { get; set; }
    public string Name { get; set; } = name;
    public int Age { get; set; } = age;
}