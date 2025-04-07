using hw3.TwoPhaseCommit.Coordinator.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace hw3.TwoPhaseCommit.Coordinator.Hubs;

public class PersonHub(IMemoryCache cache) : Hub<IPersonClient>
{
    public override Task OnConnectedAsync()
    {
        var conns = cache.Get<List<string>>("conns");
        if (conns is null)
        {
            conns = [];
            cache.Set("conns", conns);
        }
        
        conns.Add(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var conns = cache.Get<List<string>>("conns");
        if (conns is null)
        {
            conns = [];
            cache.Set("conns", conns);
            return base.OnDisconnectedAsync(exception);
        }
        
        conns.RemoveAll(conn => conn == Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}