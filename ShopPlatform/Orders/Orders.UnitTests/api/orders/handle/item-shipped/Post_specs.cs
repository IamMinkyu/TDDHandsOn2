using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Orders.Commands;
using Orders.Events;
using Xunit;

namespace Orders.api.orders.handle.item_shipped;

public class Post_specs
{
  [Fact]
  public async Task Sut_correctly_sets_event_time()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    HttpClient client = server.CreateClient();
    
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId);
    await server.HandleBankTransferPaymentCompleted(orderId);
    
    //Act
    await server.HandleItemShipped(orderId);
    ItemShipped itemShipped = new ItemShipped(
      OrderId: orderId,
      EventTimeUtc: DateTime.UtcNow);
    await client.PostAsJsonAsync($"api/orders/handle/item-shipped", itemShipped);

    HttpResponseMessage response = await client.GetAsync($"api/orders/{orderId}");
    Order? order = await response.Content.ReadFromJsonAsync<Order>();
    order!.ShippedAtUtc.Should().BeCloseTo(itemShipped.EventTimeUtc, precision: TimeSpan.FromMilliseconds(1000));
  }

  [Fact]
  public async Task Sut_returns_BadRequest_if_order_not_started()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    
    //Act
    HttpResponseMessage response = await server.HandleItemShipped(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }
  
  [Fact]
  public async Task Sut_returns_BadRequest_if_order_not_paid()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId);
    
    //Act
    HttpResponseMessage response = await server.HandleItemShipped(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Sut_returns_BadRequest_if_order_already_completed()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId);
    await server.HandleBankTransferPaymentCompleted(orderId);
    await server.HandleItemShipped(orderId);
    
    //Act
    HttpResponseMessage response = await server.HandleItemShipped(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }
}

