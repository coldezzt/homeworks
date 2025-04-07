using MassTransit;
using Npgsql;
using Dapper;
using Messaging.Contracts;
using Orders.Api;
using Orders.Api.Orders;
using Orders.Api.Outbox;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddSingleton(_ 
    => new NpgsqlDataSourceBuilder(builder.Configuration["ConnectionStrings:Database"]).Build());

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("Queue"));

        cfg.ConfigureEndpoints(context);
    });
});

DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddHostedService<OutboxBackgroundService>();
builder.Services.AddScoped<OutboxProcessor>();

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseInitializer>().Execute();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("orders", async (CreateOrderDto orderDto, NpgsqlDataSource dataSource, IPublishEndpoint publishEndpoint) =>
{
    var order = new Order
    {
        Id = Guid.NewGuid(),
        CustomerName = orderDto.CustomerName,
        ProductName = orderDto.ProductName,
        Quantity = orderDto.Quantity,
        TotalPrice = orderDto.TotalPrice,
        OrderDate = DateTime.UtcNow
    };

    const string sql =
        """
        INSERT INTO orders (id, customer_name, product_name, quantity, total_price, order_date)
        VALUES (@Id, @CustomerName, @ProductName, @Quantity, @TotalPrice, @OrderDate);
        """;

    using var connection = await dataSource.OpenConnectionAsync();
    using var transaction = await connection.BeginTransactionAsync();
    await connection.ExecuteAsync(sql, order, transaction: transaction);

    var orderCreatedEvent = new OrderCreatedIntegrationEvent(order.Id);

    // This is talking to an external component!!!
    // await publishEndpoint.Publish(orderCreatedEvent);

    await connection.InsertOutboxMessage(orderCreatedEvent, transaction: transaction);
    
    await transaction.CommitAsync();
    
    return Results.Created($"orders/{order.Id}", order);
});

app.MapGet("orders/{id:guid}", async (Guid id, NpgsqlDataSource dataSource) =>
{
    const string sql = "SELECT * FROM orders WHERE Id = @Id";

    using var connection = await dataSource.OpenConnectionAsync();
    var order = await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });

    if (order == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
});

app.Run();
