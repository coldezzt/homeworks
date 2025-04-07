using hw3.TwoPhaseCommit.Coordinator.Hubs;
using hw3.TwoPhaseCommit.Coordinator.Hubs.Clients;
using hw3.TwoPhaseCommit.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace hw3.twoPhaseCommit.coordinator.Services;

public class PseudoWorker(
    ILogger<PseudoWorker> logger,
    IMemoryCache cache,
    IHubContext<PersonHub, IPersonClient> hubContext) : BackgroundService
{
    private static readonly Random Rnd = new();

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        while (!ctx.IsCancellationRequested)
        {
            try
            {
                var conns = cache.Get<List<string>>("conns");
                if (conns is null)
                {
                    logger.LogInformation("No one connected. Waiting...");
                    await Task.Delay(15000, ctx);
                    continue;
                }

                var person = new Person {Name = "John", Age = Rnd.Next(1, 100)};

                List<Task<bool>> prepareTasks = [];
                prepareTasks.AddRange(conns.Select(conn => hubContext.Clients.Client(conn).Prepare(person)));
                Task.WaitAll(prepareTasks, ctx);
                
                if (prepareTasks.Any(t => t.Result == false))
                {
                    logger.LogInformation("Someone not prepared. Rollback...");
                    
                    var rollbackTasks = new List<Task>();
                    rollbackTasks.AddRange(conns.Select(conn => hubContext.Clients.Client(conn).Rollback()));
                    Task.WaitAll(rollbackTasks, ctx);
                    
                    logger.LogInformation("Send rollback request to all.");
                    await Task.Delay(3000, ctx);
                    continue;
                }

                logger.LogInformation("Everyone is prepared. Commiting...");
                
                List<Task> commitTasks = [];
                commitTasks.AddRange(conns.Select(conn => hubContext.Clients.Client(conn).Commit()));
                Task.WaitAll(commitTasks, ctx);
                
                logger.LogInformation("Commit complete!");
            }
            catch (Exception ex)
            {
                logger.LogError("{ex}", ex);
            }
            
            await Task.Delay(3000, ctx);
        }
    }
}