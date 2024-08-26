using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Orders.Events;
using Polly;
using Xunit;

namespace Orders.api.orders.accept.payment_approved;

public class Post_specs
{
  [Fact]
  public async Task Sut_returns_Accepted_status_code()
  {
    OrdersServer server = OrdersServer.Create();
    HttpClient client = server.CreateClient();
    string uri = "api/orders/accept/payment-approved";
    ExternalPaymentApproved body = new ($"{Guid.NewGuid()}", DateTime.UtcNow);
    
    HttpResponseMessage response = await client.PostAsJsonAsync(uri, body);
    
    response.StatusCode.Should().Be(HttpStatusCode.Accepted);
  }

  [Fact]
  public async Task Sut_correctly_changes_order_status()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    string paymentTransactionId = $"{Guid.NewGuid()}";
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId, paymentTransactionId);

    ExternalPaymentApproved paymentApproved = new(
      tid: paymentTransactionId,
      approved_at: DateTime.UtcNow);
    
    //Act
    string uri = "api/orders/accept/payment-approved";
    HttpClient client = server.CreateClient();
    await client.PostAsJsonAsync(uri, paymentApproved);
    
    //Assert
    await DefaultPolicy.Instance.ExecuteAsync(async () =>
    {
      Order? order = await server.FindOrder(orderId);
      order!.Status.Should().Be(OrderStatus.AwaitingShipment);
    });
  }

  [Fact]
  public async Task Sut_correctly_sets_event_time()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    string paymentTransactionId = $"{Guid.NewGuid()}";
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId, paymentTransactionId);

    ExternalPaymentApproved paymentApproved = new(
      tid: paymentTransactionId,
      approved_at: DateTime.UtcNow);
    
    //Act
    string uri = "api/orders/accept/payment-approved";
    HttpClient client = server.CreateClient();
    await client.PostAsJsonAsync(uri, paymentApproved);
    
    
    
    //Assert
    await DefaultPolicy.Instance.ExecuteAsync(async () =>
    {
      Order? order = await server.FindOrder(orderId);
      order!.PaidAtUtc.Should().BeCloseTo(
        paymentApproved.approved_at, 
        precision: TimeSpan.FromMilliseconds(1));
    });
  }
}
