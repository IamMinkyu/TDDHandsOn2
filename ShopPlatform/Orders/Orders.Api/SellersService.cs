namespace Orders;

public sealed class SellersService
{
  private readonly HttpClient client;
  public SellersService(HttpClient client) => this.client = client;
}
