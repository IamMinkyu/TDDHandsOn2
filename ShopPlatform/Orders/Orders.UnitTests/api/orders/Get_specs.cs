using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Orders.Commands;
using Xunit;

namespace Orders.api.orders;

public class Get_specs
{
  [Fact]
  public async Task Sut_correctly_applies_user_filter()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    HttpClient client = server.CreateClient();
    
    Guid shopId = Guid.NewGuid();
    Guid itemId = Guid.NewGuid();
    decimal price = 100000;

    List<PlaceOrder> commands = new()
    {
      new PlaceOrder(UserId: Guid.NewGuid(), shopId, itemId, price),
      new PlaceOrder(UserId: Guid.NewGuid(), shopId, itemId, price),
      new PlaceOrder(UserId: Guid.NewGuid(), shopId, itemId, price),
      new PlaceOrder(UserId: Guid.NewGuid(), shopId, itemId, price),
    };
    
    await Task.WhenAll(from command in commands
                let id = Guid.NewGuid()
                let uri = $"api/orders/{id}/place-order"
                select client.PostAsJsonAsync(uri, command));

    Guid userId = commands[0].UserId;
    
    //Act
    string queryUri = $"api/orders?userId={userId}";
    HttpResponseMessage response = await client.GetAsync(queryUri);
    Order[]? orders = await response.Content.ReadFromJsonAsync<Order[]>();
    
    //Assert
    orders!.Should().OnlyContain(x => x.UserId == userId);
  }

  [Fact]
  public async Task Sut_correctly_filter_orders_by_shop()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    HttpClient client = server.CreateClient();

    Guid userId = Guid.NewGuid();
    Guid itemId = Guid.NewGuid();
    decimal price = 10000;
    List<PlaceOrder> commands = new()
    {
      new(UserId: userId, ShopId: Guid.NewGuid(), ItemId: itemId, Price: price),
      new(UserId: userId, ShopId: Guid.NewGuid(), ItemId: itemId, Price: price),
      new(UserId: userId, ShopId: Guid.NewGuid(), ItemId: itemId, Price: price),
    };
    
    await Task.WhenAll(from command in commands
                let id = Guid.NewGuid()
                let uri = $"api/orders/{id}/place-order"
                select client.PostAsJsonAsync(uri, command));
    
    //Act
    string queryUri = $"api/orders?shopId={commands[0].ShopId}";
    Order[]? orders = await client.GetFromJsonAsync<Order[]>(queryUri);
    
    //Assert
    orders!.Should().OnlyContain(x => x.ShopId == commands[0].ShopId);
  }

  [Fact]
  public async Task Sut_correctly_filter_orders_by_user_and_shop()
  {
    OrdersServer server = OrdersServer.Create();
    HttpClient client = server.CreateClient();
    
    Guid userId = Guid.NewGuid();
    Guid shopId = Guid.NewGuid();
    Guid itemId = Guid.NewGuid();
    decimal price = 10000;
    List<PlaceOrder> commands = new()
    {
      new(UserId: userId, ShopId: Guid.NewGuid(), ItemId: itemId, Price: price),
      new(UserId: userId, ShopId: shopId, ItemId: itemId, Price: price),
      new(UserId: Guid.NewGuid(), ShopId: Guid.NewGuid(), ItemId: itemId, Price: price),
    };
    
    await Task.WhenAll(from command in commands
                let id = Guid.NewGuid()
                let uri = $"api/orders/{id}/place-order"
                select client.PostAsJsonAsync(uri, command));
    
    string queryUri = $"api/orders?userId={userId}&shopId={shopId}";
    Order[]? orders = await client.GetFromJsonAsync<Order[]>(queryUri);
    
    orders!.Should().OnlyContain(x => x.UserId == userId && x.ShopId == shopId);
  }
}
