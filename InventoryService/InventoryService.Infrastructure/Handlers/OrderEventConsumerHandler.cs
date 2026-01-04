using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.Caching.Abstractions;
using SharedService.Contracts.Events;

namespace InventoryService.Infrastructure.Handlers
{
	public class OrderEventConsumerHandler : IOrderEventConsumer
	{
		private readonly InventoryDbContext _context;
		private readonly ILogger<OrderEventConsumerHandler> _logger;
		private readonly IEventPublisher _eventPublisher;
		private readonly IRedisCacheClient _cache;

		public OrderEventConsumerHandler(
			InventoryDbContext context,
			ILogger<OrderEventConsumerHandler> logger,
			IEventPublisher eventPublisher,
			IRedisCacheClient cache)
		{
			_context = context;
			_logger = logger;
			_eventPublisher = eventPublisher;
			_cache = cache;
		}

	public async Task Handle(OrderCreatedEvent @event, Guid correlationId, Guid causationId, CancellationToken cancellationToken)
	{
		_logger.LogInformation(
			"Processing OrderCreatedEvent for OrderId: {OrderId}, CorrelationId: {CorrelationId}",
			@event.OrderId,
			correlationId);

		var failedItems = new List<(Guid ProductId, string Reason)>();
		decimal totalAmount = 0;

		foreach (var item in @event.Items)
		{
			var inventory = await _context.InventoryItems
				.FirstOrDefaultAsync(i => i.ProductId == item.ProductId, cancellationToken);

			if (inventory is null)
			{
				failedItems.Add((item.ProductId, $"Inventory not found for ProductId {item.ProductId}"));
				_logger.LogWarning("Inventory not found for ProductId {ProductId}, CorrelationId: {CorrelationId}", item.ProductId, correlationId);
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
				_logger.LogWarning("Failed to reserve inventory for ProductId {ProductId}: {Reason}, CorrelationId: {CorrelationId}", item.ProductId, ex.Message, correlationId);
			}
		}

		if (failedItems.Any())
		{
			var reason = string.Join("; ", failedItems.Select(f => $"Product {f.ProductId}: {f.Reason}"));
			await _eventPublisher.PublishInventoryReservationFailedAsync(@event.OrderId, reason, correlationId, causationId, cancellationToken);
			_logger.LogInformation("Published InventoryReservationFailed for OrderId: {OrderId}, CorrelationId: {CorrelationId}", @event.OrderId, correlationId);
			return;
		}

		await _context.SaveChangesAsync(cancellationToken);

		// Invalidate cache for all affected products
		foreach (var item in @event.Items)
		{
			await _cache.RemoveAsync($"inventory:{item.ProductId}");
		}

		// Calculate total amount from all items
		totalAmount = @event.Items.Sum(i => i.UnitPrice * i.Quantity);
		await _eventPublisher.PublishInventoryReservedAsync(@event.OrderId, totalAmount, "USD", correlationId, causationId, cancellationToken);
		_logger.LogInformation("Published InventoryReserved for OrderId: {OrderId}, TotalAmount: {TotalAmount}, CorrelationId: {CorrelationId}", @event.OrderId, totalAmount, correlationId);
	}

	public async Task HandleInventoryReleaseRequestedAsync(SharedService.Contracts.Events.Inventory.InventoryReleaseRequested @event, Guid correlationId, Guid causationId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Releasing inventory for OrderId: {OrderId}, CorrelationId: {CorrelationId}", @event.OrderId, correlationId);

		foreach (var item in @event.Items)
		{
			var inventory = await _context.InventoryItems
				.FirstOrDefaultAsync(i => i.ProductId == item.ProductId, cancellationToken);

			if (inventory != null)
			{
				inventory.Increase(item.Quantity);
				_logger.LogInformation("Released {Quantity} units of ProductId {ProductId} for OrderId {OrderId}, CorrelationId: {CorrelationId}", item.Quantity, item.ProductId, @event.OrderId, correlationId);
			}
			else
			{
				_logger.LogWarning("Inventory not found for ProductId {ProductId} when releasing for OrderId {OrderId}, CorrelationId: {CorrelationId}", item.ProductId, @event.OrderId, correlationId);
			}
		}

		await _context.SaveChangesAsync(cancellationToken);

		// Invalidate cache for all affected products
		foreach (var item in @event.Items)
		{
			await _cache.RemoveAsync($"inventory:{item.ProductId}");
		}

		_logger.LogInformation("Completed inventory release for OrderId: {OrderId}, CorrelationId: {CorrelationId}", @event.OrderId, correlationId);
	}
	}
}
