using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sellers;
using Xunit;

namespace Orders.api.orders.id.place_order;

public class Post_specs
{
  [Fact]
  public async Task Sut_returns_BadRequest_if_shop_not_exits()
  {
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    Guid shopId = Guid.NewGuid();
    HttpResponseMessage response = await server.PlaceOrder(orderId, shopId);
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }
  
  [Fact]
  public async Task Sut_returns_OK_if_shop_exits()
  {
    OrdersServer ordersServer = OrdersServer.Create();
    HttpResponseMessage response = await 
      ordersServer.PlaceOrder(orderId: Guid.NewGuid());
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }
  
  [Fact]
  public async Task Sut_does_not_create_order_if_shop_not_exits()
  {
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    Guid shopId = Guid.NewGuid();
    await server.PlaceOrder(orderId, shopId);

    HttpResponseMessage response = await server.CreateClient().GetAsync($"api/orders/{orderId}");
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }
}
