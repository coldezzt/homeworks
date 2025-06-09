var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Fusion");

builder.Services
    .AddFusionGatewayServer()
    .ConfigureFromFile("gateway.fgp")
    .ModifyFusionOptions(x => x.AllowQueryPlan = true);

var app = builder.Build();
app.MapGraphQL();
app.Run();