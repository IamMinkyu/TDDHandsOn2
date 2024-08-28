using FluentAssertions;
using Sellers;
using Xunit;

namespace Orders.api.orders.id;

public class Get_specs
{
  [Fact]
  public async Task Sut_correctly_sets_shop_name()
  {
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    
    Order? actual = await server.FindOrder(orderId);
    ShopView shop = await server.GetSellersServer().GetShop(actual!.ShopId);
    
    actual.ShopName.Should().Be(shop!.Name);
  }
}
