namespace hw3.Saga.Shared.Models;

public class BuyItemsResponse
{
    public Guid OrderId { get; set; }
    public string ErrorMessage { get; set; }
}