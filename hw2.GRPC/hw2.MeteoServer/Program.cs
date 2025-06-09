using MeteoService = MeteoServer.Services.MeteoService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<MeteoService>();

app.Run();