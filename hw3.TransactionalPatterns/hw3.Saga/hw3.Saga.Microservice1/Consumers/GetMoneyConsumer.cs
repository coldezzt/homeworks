using hw3.Saga.Shared.Models;
using MassTransit;

namespace hw3.Saga.Microservice1.Consumers;

public class GetMoneyConsumer : IConsumer<GetMoneyRequest>
{
    public Task Consume(ConsumeContext<GetMoneyRequest> context)
    {
        return context.RespondAsync<GetMoneyResponse>(new { context.Message.OrderId });
    }
}