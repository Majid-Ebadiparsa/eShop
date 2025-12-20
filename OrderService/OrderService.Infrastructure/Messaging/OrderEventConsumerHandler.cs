using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Repositories.EF;
using SharedService.Contracts.Events;
using SharedService.Contracts.Events.Inventory;
using SharedService.Contracts.Events.Payment;

namespace OrderService.Infrastructure.Messaging
{
	public class OrderEventConsumerHandler : IOrderEventConsumer
	{
		private readonly OrderDbContext _context;
		private readonly ILogger<OrderEventConsumerHandler> _logger;
		private readonly IEventPublisher _eventPublisher;

		public OrderEventConsumerHandler(OrderDbContext context, ILogger<OrderEventConsumerHandler> logger, IEventPublisher eventPublisher)
		{
			_context = context;
			_logger = logger;
			_eventPublisher = eventPublisher;
		}

		public async Task HandleInventoryReservedAsync(InventoryReserved @event, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

			if (order == null)
			{
				_logger.LogWarning("Order not found for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkInventoryReserved();
			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Order {OrderId} marked as InventoryReserved", @event.OrderId);
		}

		public async Task HandleInventoryReservationFailedAsync(InventoryReservationFailed @event, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

			if (order == null)
			{
				_logger.LogWarning("Order not found for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkInventoryReservationFailed();
			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Order {OrderId} marked as InventoryReservationFailed", @event.OrderId);
		}

		public async Task HandlePaymentAuthorizedAsync(PaymentAuthorized @event, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

			if (order == null)
			{
				_logger.LogWarning("Order not found for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkPaymentAuthorized();
			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Order {OrderId} marked as PaymentAuthorized", @event.OrderId);
		}

		public async Task HandlePaymentCapturedAsync(PaymentCaptured @event, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

			if (order == null)
			{
				_logger.LogWarning("Order not found for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkPaymentCaptured();
			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Order {OrderId} marked as PaymentCaptured", @event.OrderId);
		}

		public async Task HandlePaymentFailedAsync(PaymentFailed @event, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

			if (order == null)
			{
				_logger.LogWarning("Order not found for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkPaymentFailed();
			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Order {OrderId} marked as PaymentFailed", @event.OrderId);

			// Compensating action: Release inventory if payment fails after inventory was reserved
			if (order.Status == OrderService.Domain.SeedWork.OrderStatus.PaymentFailed)
			{
				var items = order.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
				await _eventPublisher.PublishInventoryReleaseRequestedAsync(@event.OrderId, items);
				_logger.LogInformation("Published InventoryReleaseRequested for OrderId: {OrderId}", @event.OrderId);
			}
		}

		public async Task HandleShipmentCreatedAsync(ShipmentCreated @event, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

			if (order == null)
			{
				_logger.LogWarning("Order not found for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkShipmentCreated();
			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Order {OrderId} marked as ShipmentCreated", @event.OrderId);
		}

		public async Task HandleShipmentDispatchedAsync(ShipmentDispatched @event, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

			if (order == null)
			{
				_logger.LogWarning("Order not found for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkShipmentDispatched();
			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Order {OrderId} marked as ShipmentDispatched", @event.OrderId);
		}

		public async Task HandleShipmentDeliveredAsync(ShipmentDelivered @event, CancellationToken cancellationToken)
		{
			var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == @event.OrderId, cancellationToken);

			if (order == null)
			{
				_logger.LogWarning("Order not found for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			order.MarkDelivered();
			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Order {OrderId} marked as Delivered", @event.OrderId);
		}
	}
}

