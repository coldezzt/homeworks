using hw3.Saga.Shared.Enums;
using hw3.Saga.Shared.Models;
using MassTransit;

namespace hw3.Saga.SagaService.Sagas;

public sealed class BuyItemsSaga : MassTransitStateMachine<BuyItemsSagaState>
{
    private readonly ILogger<BuyItemsSaga> _logger;

    public BuyItemsSaga(ILogger<BuyItemsSaga> logger)
    {
        _logger = logger;
        InstanceState(x => x.CurrentState);
        Event(() => BuyItems, x => x.CorrelateById(y => y.Message.OrderId));
        Request(() => GetMoney);
        Request(() => GetItems);
        Initially(
            When(BuyItems)
                .Then(x =>
                {
                    if (!x.TryGetPayload(out SagaConsumeContext<BuyItemsSagaState, BuyItemsRequest>? payload))
                        throw new Exception("Unable to retrieve required payload for callback data.");
                    x.Saga.RequestId = payload.RequestId;
                    x.Saga.ResponseAddress = payload.ResponseAddress;
                })
                .Request(GetMoney, x => x.Init<GetMoneyRequest>(new {x.Message.OrderId }))
                .TransitionTo(GetMoney?.Pending)
            );

        During(GetMoney?.Pending,
            
            When(GetMoney?.Completed)
                .Request(GetItems, x => x.Init<GetItemsRequest>(new {x.Message.OrderId }))
                .TransitionTo(GetItems?.Pending),
            
            When(GetMoney?.Faulted)
                .ThenAsync(async context =>
                { 
                  await RespondFromSaga(context, 
                      "Faulted On Get Money " + string.Join("; ", context.Message.Exceptions.Select(x => x.Message)));
                })
                .TransitionTo(Failed),
            
            When(GetMoney?.TimeoutExpired)
                .ThenAsync(async context =>
                   await RespondFromSaga(context, "Timeout Expired On Get Money"))
                .TransitionTo(Failed)
            );

        During(GetItems?.Pending,
            
            When(GetItems?.Completed)
                .ThenAsync(async context =>
                {
                    await RespondFromSaga(context, "Всё файн");
                })
                .Finalize(),

            When(GetItems?.Faulted)
                .ThenAsync(async context =>
                {
                    await RespondFromSaga(context, 
                        "Faulted On Get Items " + string.Join("; ", context.Message.Exceptions.Select(x => x.Message)));
                })
                .TransitionTo(Failed),

            When(GetItems?.TimeoutExpired)
                .ThenAsync(async context =>
                {
                    await RespondFromSaga(context, "Timeout Expired On Get Items");
                })
                .TransitionTo(Failed)
            );
    }
    public Request<BuyItemsSagaState, GetMoneyRequest, GetMoneyResponse> GetMoney { get; set; }
    public Request<BuyItemsSagaState, GetItemsRequest, GetItemsResponse> GetItems { get; set; }
    public Event<BuyItemsRequest> BuyItems { get; set; }
    public State Failed { get; set; }
    private static async Task RespondFromSaga<T>(BehaviorContext<BuyItemsSagaState, T> context, string error) where T : class
    {
        if (context.Saga.ResponseAddress != null)
        {
            var endpoint = await context.GetSendEndpoint(context.Saga.ResponseAddress);
            await endpoint.Send(new BuyItemsResponse
            {
                OrderId = context.Saga.CorrelationId,
                ErrorMessage = error
            }, r => r.RequestId = context.Saga.RequestId);
        }
    }
}