using hw1.Subscription;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<RandomNumberPublisher>();

builder.Services
    .AddGraphQLServer()
    .ModifyOptions(o => o.StrictValidation = false)
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions();

var app = builder.Build();
app.UseWebSockets();
app.MapGraphQL();
app.RunWithGraphQLCommands(args);

public class Subscription
{
    [Subscribe]
    [Topic("randomNumber")]
    public int OnRandomNumber([EventMessage] int number) => number;
}