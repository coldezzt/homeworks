using hw3.Saga.Shared.Models;

namespace hw3.Saga.Gateway.Models;

public class BuyItemsRequstModel : BuyItemsRequest
{
    public Guid OrderId { get; set; }

    public int Count { get; set; }

    public decimal Amount { get; set; }
}
