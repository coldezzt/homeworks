using MassTransit;

namespace hw3.Saga.Shared.Enums;

public sealed class BuyItemsSagaState : SagaStateMachineInstance
{
    //Идентификатор по которому мы отличаем один процесс от другого.
    public Guid CorrelationId { get; set; }
    //Текущее состояние саги ака Failed, GetItemsPending и т.д.
    public string? CurrentState { get; set; }
    //Тут мы сохраняем идентификатор запроса что запустил нашу сагу
    //чтобы ответить на него
    public Guid? RequestId { get; set; }
    //Тут мы сохраняем адрес откуда пришел запрос который запустил нашу сагу
    //чтобы ответить на него
    public Uri? ResponseAddress { get; set; }
}