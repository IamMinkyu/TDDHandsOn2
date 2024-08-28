using System.Net.Http.Json;

namespace Sellers;

public static class TestSpecifiedLanguage
{
  public static async Task<Shop> CreateShop(this SellersServer server)
  {
    HttpClient client = server.CreateClient();
    string uri = "api/shops/";
    HttpResponseMessage response = await client.PostAsJsonAsync(uri, new { Name = $"{Guid.NewGuid()}"});
    return (await response.Content.ReadFromJsonAsync<Shop>())!;
  }
  
  public static Task SetShopUser(this SellersServer server, Guid shopId, string userId, string password)
  {
    HttpClient client = server.CreateClient();
    string uri = $"api/shops/{shopId}/user";
    ShopUser body = new ShopUser(userId, password);
    return client.PostAsJsonAsync(uri, body);
  }
}
