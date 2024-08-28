using System.Net;
using FluentAssertions;
using Xunit;

namespace Orders.api.orders.id.start_order;

public class Post_specs
{
  [Fact]
  public async Task Sut_returns_BadRequest_if_already_started()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId);
    
    //Act
    var response = await server.StartOrder(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }
  
  [Fact]
  public async Task Sut_returns_BadRequest_if_payment_completed()
  {
    //Arrange
    var server = OrdersServer.Create();
    
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId);
    await server.HandleBankTransferPaymentCompleted(orderId);
    
    //Act
    var response = await server.StartOrder(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }
  
  [Fact]
  public async Task Sut_returns_BadRequest_if_order_completed()
  {
    //Arrange
    var server = OrdersServer.Create();
    
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId);
    await server.HandleBankTransferPaymentCompleted(orderId);
    await server.HandleItemShipped(orderId);
    
    //Act
    HttpResponseMessage response = await server.StartOrder(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Sut_correctly_sets_payment_transaction_id()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    Guid orderId = Guid.NewGuid();
    string paymentTransactionId = $"{Guid.NewGuid()}";
    await server.PlaceOrder(orderId);
    
    //Act
    await server.StartOrder(orderId, paymentTransactionId);
    Order? actual = await server.FindOrder(orderId);

    //Assert
    actual!.PaymentTransactionId.Should().Be(paymentTransactionId);

  }
}
