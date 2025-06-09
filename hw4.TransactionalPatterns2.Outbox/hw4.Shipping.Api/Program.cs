using Dapper;
using MassTransit;
using Npgsql;
using Shipping.Api;
using Shipping.Api.Shipments;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddSingleton(_ =>
{
    return new NpgsqlDataSourceBuilder(builder.Configuration["ConnectionStrings:Database"]).Build();
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedIntegrationEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("Queue"));

        cfg.ConfigureEndpoints(context);
    });
});

DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseInitializer>().Execute();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("shipments", async (Guid orderId, NpgsqlDataSource dataSource) =>
{
    const string sql = "SELECT * FROM shipments WHERE order_id = @OrderId";

    await using var connection = await dataSource.OpenConnectionAsync();
    var shipments = await connection.QueryAsync<Shipment>(sql, new { OrderId = orderId });

    if (!shipments.Any())
    {
        return Results.NotFound();
    }

    return Results.Ok(shipments);
});

app.MapGet("shipments/{id:guid}", async (Guid id, NpgsqlDataSource dataSource) =>
{
    const string sql = "SELECT * FROM shipments WHERE id = @Id";

    await using var connection = await dataSource.OpenConnectionAsync();
    var shipment = await connection.QuerySingleOrDefaultAsync<Shipment>(sql, new { Id = id });

    if (shipment == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(shipment);
});

app.MapPost("shipments/{id:guid}/ship", async (Guid id, NpgsqlDataSource dataSource) =>
{
    const string sql =
        """
        UPDATE shipments 
        SET status = @Status, updated_at = @UpdatedAt
        WHERE id = @Id AND status = @PendingStatus
        RETURNING *;
        """;

    await using var connection = await dataSource.OpenConnectionAsync();
    var shipment = await connection.QuerySingleOrDefaultAsync<Shipment>(sql, new 
    { 
        Id = id, 
        Status = ShipmentStatus.Shipped.ToString(),
        UpdatedAt = DateTime.UtcNow,
        PendingStatus = ShipmentStatus.Pending.ToString()
    });

    if (shipment == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(shipment);
});

app.Run();
