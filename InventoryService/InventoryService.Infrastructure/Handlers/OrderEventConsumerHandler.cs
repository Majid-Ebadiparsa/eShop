using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events;

namespace InventoryService.Infrastructure.Handlers
{
	public class OrderEventConsumerHandler : IOrderEventConsumer
	{
		private readonly InventoryDbContext _context;
		private readonly ILogger<OrderEventConsumerHandler> _logger;
		private readonly IEventPublisher _eventPublisher;

		public OrderEventConsumerHandler(
			InventoryDbContext context,
			ILogger<OrderEventConsumerHandler> logger,
			IEventPublisher eventPublisher)
		{
			_context = context;
			_logger = logger;
			_eventPublisher = eventPublisher;
		}

		public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
		{
			var failedItems = new List<(Guid ProductId, string Reason)>();
			decimal totalAmount = 0;

			foreach (var item in @event.Items)
			{
				var inventory = await _context.InventoryItems
					.FirstOrDefaultAsync(i => i.ProductId == item.ProductId, cancellationToken);

				if (inventory is null)
				{
					failedItems.Add((item.ProductId, $"Inventory not found for ProductId {item.ProductId}"));
					_logger.LogWarning("Inventory not found for ProductId {ProductId}", item.ProductId);
					continue;
				}

				try
				{
					inventory.Decrease(item.Quantity);
					totalAmount += item.UnitPrice * item.Quantity;
				}
				catch (InvalidOperationException ex)
				{
					failedItems.Add((item.ProductId, ex.Message));
					_logger.LogWarning("Failed to reserve inventory for ProductId {ProductId}: {Reason}", item.ProductId, ex.Message);
				}
			}

			if (failedItems.Any())
			{
				var reason = string.Join("; ", failedItems.Select(f => $"Product {f.ProductId}: {f.Reason}"));
				await _eventPublisher.PublishInventoryReservationFailedAsync(@event.OrderId, reason, cancellationToken);
				_logger.LogInformation("Published InventoryReservationFailed for OrderId: {OrderId}", @event.OrderId);
				return;
			}

			await _context.SaveChangesAsync(cancellationToken);

			// Calculate total amount from all items
			totalAmount = @event.Items.Sum(i => i.UnitPrice * i.Quantity);
			await _eventPublisher.PublishInventoryReservedAsync(@event.OrderId, totalAmount, "USD", cancellationToken);
			_logger.LogInformation("Published InventoryReserved for OrderId: {OrderId}, TotalAmount: {TotalAmount}", @event.OrderId, totalAmount);
		}

		public async Task HandleInventoryReleaseRequestedAsync(SharedService.Contracts.Events.Inventory.InventoryReleaseRequested @event, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Releasing inventory for OrderId: {OrderId}", @event.OrderId);

			foreach (var item in @event.Items)
			{
				var inventory = await _context.InventoryItems
					.FirstOrDefaultAsync(i => i.ProductId == item.ProductId, cancellationToken);

				if (inventory != null)
				{
					inventory.Increase(item.Quantity);
					_logger.LogInformation("Released {Quantity} units of ProductId {ProductId} for OrderId {OrderId}", item.Quantity, item.ProductId, @event.OrderId);
				}
				else
				{
					_logger.LogWarning("Inventory not found for ProductId {ProductId} when releasing for OrderId {OrderId}", item.ProductId, @event.OrderId);
				}
			}

			await _context.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Completed inventory release for OrderId: {OrderId}", @event.OrderId);
		}
	}
}
