using HotChocolate.Subscriptions;

namespace hw1.Subscription;

public class RandomNumberPublisher(ITopicEventSender eventSender) : BackgroundService
{
    private static readonly Random Rnd = new((int)DateTime.Now.ToBinary());
    
    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        while (!ctx.IsCancellationRequested)
        {
            await Task.Delay(1000, ctx);
            var n = Rnd.Next();
            await eventSender.SendAsync("randomNumber", n, ctx);
            Console.WriteLine($"Created: {n}");
        }
    }
}