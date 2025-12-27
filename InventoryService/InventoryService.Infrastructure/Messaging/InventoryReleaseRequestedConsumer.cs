using InventoryService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events.Inventory;
using InventoryService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Messaging
{
	public class InventoryReleaseRequestedConsumer : IConsumer<InventoryReleaseRequested>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<InventoryReleaseRequestedConsumer> _logger;
		private readonly InventoryDbContext _db;

		public InventoryReleaseRequestedConsumer(IOrderEventConsumer handler, ILogger<InventoryReleaseRequestedConsumer> logger, InventoryDbContext db)
		{
			_handler = handler;
			_logger = logger;
			_db = db;
		}

		public async Task Consume(ConsumeContext<InventoryReleaseRequested> context)
		{
			_logger.LogInformation("Received InventoryReleaseRequested for OrderId: {OrderId}", context.Message.OrderId);
			var messageId = context.MessageId ?? Guid.NewGuid();
			var consumerName = nameof(InventoryReleaseRequestedConsumer).ToLowerInvariant();

			var already = await _db.ProcessedMessages.AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, context.CancellationToken);
			if (already)
			{
				_logger.LogInformation("Skipping already processed message {MessageId}", messageId);
				return;
			}

			await using var tx = await _db.Database.BeginTransactionAsync(context.CancellationToken);
			try
			{
				await _handler.HandleInventoryReleaseRequestedAsync(context.Message, context.CancellationToken);

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

