using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Sellers.api.shops.id;

public class Get_specs
{
  [Fact]
  public async Task Sut_does_not_expose_user_information()
  {
    SellersServer server = SellersServer.Create();
    Shop shop = await server.CreateShop();
    await server.SetShopUser(shop!.Id, $"{Guid.NewGuid()}", "password 1");
    
    string uri = $"api/shops/{shop.Id}";
    HttpResponseMessage response = await server.CreateClient().GetAsync(uri);
    Shop? actual = await response.Content.ReadFromJsonAsync<Shop>();
    
    actual!.UserId.Should().BeNull();
    actual!.PasswordHash.Should().BeNull();
  }

  
}
