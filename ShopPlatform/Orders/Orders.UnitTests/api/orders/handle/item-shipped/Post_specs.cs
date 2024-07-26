using System.Net.Http.Json;
using FluentAssertions;
using Orders.Commands;
using Orders.Events;
using Xunit;

namespace Orders.api.orders.handle.item_shipped;

public class Post_specs
{
  public Post_specs()
  {
  }

  [Fact]
  public async Task Sut_collrectly_sets_event_time()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();

    Guid orderId = Guid.NewGuid();
    PlaceOrder placeOrder = new PlaceOrder(
      UserId: Guid.NewGuid(),
      ShopId: Guid.NewGuid(),
      ItemId: Guid.NewGuid(),
      Price: 100000);
    
    HttpClient client = server.CreateClient();
    await client.PostAsJsonAsync($"api/orders/{orderId}/place-order", placeOrder);
    
    StartOrder startOrder = new StartOrder();
    await client.PostAsJsonAsync($"api/orders/{orderId}/start-order", startOrder);

    BankTransferPaymentCompleted bankTransferPaymentCompleted = new(
      OrderId: orderId, 
      EventTimeUtc: DateTime.UtcNow);
    await client.PostAsJsonAsync($"api/orders/handle/bank-transfer-payment-completed", bankTransferPaymentCompleted);
    
    //Act
    DateTime eventTime = DateTime.UtcNow;
    ItemShipped itemShipped = new ItemShipped(
      OrderId: orderId,
      EventTimeUtc: DateTime.UtcNow);
    await client.PostAsJsonAsync($"api/orders/handle/item-shipped", itemShipped);

    HttpResponseMessage response = await client.GetAsync($"api/orders/{orderId}");
    Order? order = await response.Content.ReadFromJsonAsync<Order>();
    order!.ShippedAtUtc.Should().BeCloseTo(eventTime, precision: TimeSpan.FromSeconds(1));
  }
}

