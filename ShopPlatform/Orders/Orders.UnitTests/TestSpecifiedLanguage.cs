using System.Net.Http.Json;
using Orders.Commands;
using Orders.Events;

namespace Orders;

public static class TestSpecifiedLanguage
{
  public static async Task PlaceOrder(this OrdersServer server, Guid orderId)
  {
    var client = server.CreateClient();
    var uri = $"api/orders/{orderId}/place-order";
    
    PlaceOrder placeOrder = new(
      UserId: Guid.NewGuid(),
      ShopId: Guid.NewGuid(),
      ItemId: Guid.NewGuid(),
      Price: 100000);
    
    await client.PostAsJsonAsync(uri, placeOrder);
  }
  
  public static async Task<HttpResponseMessage> StartOrder(this OrdersServer server, Guid orderId)
  {
    var client = server.CreateClient();
    var uri = $"api/orders/{orderId}/start-order";
    StartOrder body = new();
    return await client.PostAsJsonAsync(uri, body);
  }
  
  public static async Task<HttpResponseMessage> HandleBankTransferPaymentCompleted(this OrdersServer server, Guid orderId)
  {
    var client = server.CreateClient();
    var uri = $"api/orders/handle/bank-transfer-payment-completed";
    BankTransferPaymentCompleted body = new(orderId, DateTime.UtcNow);
    return await client.PostAsJsonAsync(uri, body);
  }
  
  public static async Task<HttpResponseMessage> HandleItemShipped(this OrdersServer server, Guid orderId)
  {
    var client = server.CreateClient();
    var uri = $"api/orders/handle/item-shipped";
    ItemShipped body = new(orderId, DateTime.UtcNow);
    return await client.PostAsJsonAsync(uri, body);
  }
}
