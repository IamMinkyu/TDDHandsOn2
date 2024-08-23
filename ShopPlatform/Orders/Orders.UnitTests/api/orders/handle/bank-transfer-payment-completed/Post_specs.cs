using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Orders.Commands;
using Orders.Events;
using Xunit;

namespace Orders.api.orders.handle.bank_transfer_payment_completed;

public class Post_specs
{
  [Fact]
  public async Task Sut_returns_BadRequest_if_order_not_started()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    
    //Act
    HttpResponseMessage response = await server.HandleBankTransferPaymentCompleted(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Sut_returns_BadRequest_if_order_already_payment_completed()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId);
    await server.HandleBankTransferPaymentCompleted(orderId);
    
    //Act
    var response = await server.HandleBankTransferPaymentCompleted(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Sut_returns_BadRequest_if_order_completed()
  {
    //Arrange
    OrdersServer server = OrdersServer.Create();
    
    Guid orderId = Guid.NewGuid();
    await server.PlaceOrder(orderId);
    await server.StartOrder(orderId);
    await server.HandleBankTransferPaymentCompleted(orderId);
    await server.HandleItemShipped(orderId);
    
    //Act
    var response = await server.HandleBankTransferPaymentCompleted(orderId);
    
    //Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }
}
