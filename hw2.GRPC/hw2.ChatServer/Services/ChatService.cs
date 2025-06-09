using System.Collections.Concurrent;
using ChatGrpc;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace hw2.ChatServer.Services;

public class ChatService : ChatGrpc.ChatService.ChatServiceBase
{
    private static readonly List<User> KnownUsers = [];
    private static readonly ConcurrentDictionary<string, User> ActiveUsers = [];
    private static readonly List<IServerStreamWriter<ChatMessage>> Observers = [];
    
    public override Task<JoinResponse> Join(User u, ServerCallContext context)
    {
        var chatUser = ActiveUsers.FirstOrDefault(au => au.Key == u.Name).Key;
        if (chatUser is not null)
            return Task.FromResult(new JoinResponse
            {
                Error = 1,
                Msg = "User already joined"
            });
        
        ActiveUsers.TryAdd(u.Name, u);
        return Task.FromResult(new JoinResponse
        {
            Error = 0,
            Msg = "Success"
        });
    }

    public override async Task ReceiveMsg(Empty request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
    {
        Observers.Add(responseStream);

        while (!context.CancellationToken.IsCancellationRequested) { }
        
        Observers.Remove(responseStream);
    }

    public override async Task<Empty> SendMsg(ChatMessage c, ServerCallContext context)
    {
        var toRemove = new List<IServerStreamWriter<ChatMessage>>();
        foreach (var o in Observers)
        {
            try
            {
                await o.WriteAsync(c);
            }
            catch (Exception e)
            {
                if (e is InvalidOperationException)
                    toRemove.Add(o);
                else Console.WriteLine(e.Message);
            }
        }
        
        Observers.RemoveAll(o => toRemove.Contains(o));
        return new Empty();
    }

    public override Task<UserList> GetAllUsers(Empty request, ServerCallContext context)
    {
        var resp = new UserList();
        resp.Users.AddRange(ActiveUsers.Values);
        return Task.FromResult(resp);
    }
}