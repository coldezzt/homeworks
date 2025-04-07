using hw3.Saga.Shared.Models;

namespace hw3.Saga.Gateway.Models;

public class BuyItemsResponseModel : BuyItemsResponse
{
    public Guid OrderId { get; set; }

    public string ErrorMessage { get; set; }
}
