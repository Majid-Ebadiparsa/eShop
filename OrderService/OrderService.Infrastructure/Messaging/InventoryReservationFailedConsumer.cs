using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Inventory;
using OrderService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Messaging
{
	public class InventoryReservationFailedConsumer : IConsumer<InventoryReservationFailed>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<InventoryReservationFailedConsumer> _logger;
		private readonly OrderDbContext _db;

		public InventoryReservationFailedConsumer(IOrderEventConsumer handler, ILogger<InventoryReservationFailedConsumer> logger, OrderDbContext db)
		{
			_handler = handler;
			_logger = logger;
			_db = db;
		}

		public async Task Consume(ConsumeContext<InventoryReservationFailed> context)
		{
			_logger.LogInformation("Received InventoryReservationFailed for OrderId: {OrderId}", context.Message.OrderId);
			var messageId = context.MessageId ?? Guid.NewGuid();
			var consumerName = nameof(InventoryReservationFailedConsumer).ToLowerInvariant();

			var already = await _db.ProcessedMessages.AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, context.CancellationToken);
			if (already)
			{
				_logger.LogInformation("Skipping already processed message {MessageId}", messageId);
				return;
			}

			await using var tx = await _db.Database.BeginTransactionAsync(context.CancellationToken);
			try
			{
				await _handler.HandleInventoryReservationFailedAsync(context.Message, context.CancellationToken);

				_db.ProcessedMessages.Add(new ProcessedMessage
				{
					MessageId = messageId,
					ConsumerName = consumerName,
					CorrelationId = context.CorrelationId,
					ProcessedAt = DateTime.UtcNow
				});

				await _db.SaveChangesAsync(context.CancellationToken);
				await tx.CommitAsync(context.CancellationToken);
			}
			catch (Exception ex)
			{
				await tx.RollbackAsync(context.CancellationToken);
				_logger.LogError(ex, "Error processing message {MessageId}", messageId);
				throw;
			}
		}
	}
}

