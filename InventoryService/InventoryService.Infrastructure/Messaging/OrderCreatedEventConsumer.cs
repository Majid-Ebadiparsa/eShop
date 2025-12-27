using InventoryService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events;
using InventoryService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Messaging
{
	public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<OrderCreatedEventConsumer> _logger;
		private readonly InventoryDbContext _db;

		public OrderCreatedEventConsumer(IOrderEventConsumer handler, ILogger<OrderCreatedEventConsumer> logger, InventoryDbContext db)
		{
			_handler = handler;
			_logger = logger;
			_db = db;
		}

		public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
		{
			_logger.LogInformation("Received OrderCreatedEvent for OrderId: {OrderId}", context.Message.OrderId);
			var messageId = context.MessageId ?? Guid.NewGuid();
			var consumerName = nameof(OrderCreatedEventConsumer).ToLowerInvariant();

			var already = await _db.ProcessedMessages.AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, context.CancellationToken);
			if (already)
			{
				_logger.LogInformation("Skipping already processed message {MessageId}", messageId);
				return;
			}

			await using var tx = await _db.Database.BeginTransactionAsync(context.CancellationToken);
			try
			{
				await _handler.Handle(context.Message, context.CancellationToken);

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
