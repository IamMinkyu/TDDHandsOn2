using Microsoft.EntityFrameworkCore;
using Orders.Messaging;

namespace Orders.Events;

public class PaymentApprovedEventHandler
{
  public static void Listen(IServiceProvider services)
  {
    var stream = services.GetRequiredService<IAsyncObservable<PaymentApproved>>();
    
    stream.Subscribe(async listenedEvent =>
    {
      using IServiceScope scope = services.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
      IQueryable<Order> query =
        from x in context.Orders
        where x.PaymentTransactionId == listenedEvent.PaymentTransactionId
        select x;
      if (await query.SingleOrDefaultAsync() is Order order)
      {
        order.Status = OrderStatus.AwaitingShipment;
        order.PaidAtUtc = listenedEvent.EventTimeUtc;
        await context.SaveChangesAsync();
      }
    });
  }
}
