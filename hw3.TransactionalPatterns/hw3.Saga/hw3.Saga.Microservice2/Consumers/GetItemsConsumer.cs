using hw3.Saga.Shared.Models;
using MassTransit;

namespace hw3.Saga.Microservice2.Consumers;

public class GetItemsConsumer : IConsumer<GetItemsRequest>
{
    public Task Consume(ConsumeContext<GetItemsRequest> context)
    {
        return context.RespondAsync<GetItemsResponse>(new { context.Message.OrderId });
    }
}