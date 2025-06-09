using hw3.TwoPhaseCommit.Coordinator.Hubs;
using hw3.twoPhaseCommit.coordinator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMemoryCache()
    .AddHostedService<PseudoWorker>()
    .AddSignalR();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapHub<PersonHub>("/person");

app.Run();