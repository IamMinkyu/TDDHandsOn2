﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Commands;
using Orders.Events;
using Orders.Messaging;

namespace Orders.Controllers;

[Route("api/orders")]
public sealed class OrdersController : Controller
{
    [HttpGet]
    [Produces("application/json", Type = typeof(Order[]))]
    public async Task<IEnumerable<Order>> GetOrders(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? shopId,
        [FromServices] OrdersDbContext context)
    {
        return await context.Orders.AsNoTracking()
            .FilterByUserId(userId)
            .FilterByShopId(shopId)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    [Produces("application/json", Type = typeof(Order))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> FindOrder(
        Guid id,
        [FromServices] OrdersDbContext context)
    {
        return await context.Orders.FindOrder(id) switch
        {
            Order order => Ok(order),
            null => NotFound(),
        };
    }

    [HttpPost("{id}/place-order")]
    public async Task<IActionResult> PlaceOrder(
        Guid id,
        [FromBody] PlaceOrder command,
        [FromServices] SellersService sellersService,
        [FromServices] OrdersDbContext context)
    {
        if (await sellersService.ShopExists(command.ShopId))
        {
            context.Add(new Order
            {
                Id = id,
                UserId = command.UserId,
                ShopId = command.ShopId,
                ItemId = command.ItemId,
                Price = command.Price,
                Status = OrderStatus.Pending,
                PlacedAtUtc = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpPost("{id}/start-order")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> StartOrder(
        Guid id,
        [FromBody] StartOrder command,
        [FromServices] OrdersDbContext context)
    {
        Order? order = await context.Orders.FindOrder(id);

        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.Pending)
        {
            return BadRequest();
        }
        

        order.Status = OrderStatus.AwaitingPayment;
        order.PaymentTransactionId = command.PaymentTransactionId;
        order.StartedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("handle/bank-transfer-payment-completed")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> HandleBankTransferPaymentCompleted(
        [FromBody] BankTransferPaymentCompleted listenedEvent,
        [FromServices] OrdersDbContext context)
    {
        Order? order = await context.Orders.FindOrder(listenedEvent.OrderId);

        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.AwaitingPayment)
        {
            return BadRequest();
        }

        order.Status = OrderStatus.AwaitingShipment;
        order.PaidAtUtc = listenedEvent.EventTimeUtc;
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("handle/item-shipped")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> HandleItemShipped(
        [FromBody] ItemShipped listenedEvent,
        [FromServices] OrdersDbContext context)
    {
        Order? order = await context.Orders.FindOrder(listenedEvent.OrderId);

        if (order == null)
        {
            return NotFound();
        }

        if (order.Status != OrderStatus.AwaitingShipment)
        {
            return BadRequest();
        }

        order.Status = OrderStatus.Completed;
        order.ShippedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpPost("accept/payment-approved")]
    public async Task<IActionResult> AcceptPaymentApproved(
        [FromBody] ExternalPaymentApproved listenedEvent,
        [FromServices] IBus<PaymentApproved> bus)
    {
        PaymentApproved message = new (
            PaymentTransactionId: listenedEvent.tid, 
            EventTimeUtc: listenedEvent.approved_at);
        await bus.Send(message);
        return Accepted();
    }
    
}
