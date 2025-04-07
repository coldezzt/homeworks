using hw3.Saga.Gateway.Models;
using hw3.Saga.Shared.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace hw3.Saga.Gateway.Controllers;

[ApiController]
[Route("api/v1/items")]
public class ItemsController(IBus bus, ILogger<ItemsController> logger) : ControllerBase
{
    [HttpPost("buy")]
    public async Task<BuyItemsResponse> BuyAsync(BuyItemsRequstModel model)
    {
        logger.LogWarning("HTTTTTT!!!");
        var response = await bus.Request<BuyItemsRequest, BuyItemsResponse>(model);
        return response.Message;
    }
}