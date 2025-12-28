using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Inventory;
using OrderService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;


namespace OrderService.Infrastructure.Messaging
{
	public class InventoryReservedConsumer : IConsumer<InventoryReserved>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<InventoryReservedConsumer> _logger;
		private readonly OrderDbContext _db;

		public InventoryReservedConsumer(IOrderEventConsumer handler, ILogger<InventoryReservedConsumer> logger, OrderDbContext db)
		{
			_handler = handler;
			_logger = logger;
			_db = db;
		}

		public async Task Consume(ConsumeContext<InventoryReserved> context)
		{
			_logger.LogInformation("Received InventoryReserved for OrderId: {OrderId}", context.Message.OrderId);

			var messageId = context.MessageId ?? Guid.NewGuid();
			var consumerName = nameof(InventoryReservedConsumer).ToLowerInvariant();

			// Check if already processed
			var already = await _db.ProcessedMessages.AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, context.CancellationToken);
			if (already)
			{
				_logger.LogInformation("Skipping already processed message {MessageId}", messageId);
				return;
			}

			// Use DB transaction to ensure handler DB work and processed marker are atomic
			await using var tx = await _db.Database.BeginTransactionAsync(context.CancellationToken);
			try
			{
				await _handler.HandleInventoryReservedAsync(context.Message, context.CancellationToken);

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

