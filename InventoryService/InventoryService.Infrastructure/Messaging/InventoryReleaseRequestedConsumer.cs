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
		var message = context.Message;
		var messageId = message.MessageId;
		var correlationId = message.CorrelationId;

		_logger.LogInformation(
			"Received InventoryReleaseRequested for OrderId: {OrderId}, CorrelationId: {CorrelationId}, MessageId: {MessageId}",
			message.OrderId,
			correlationId,
			messageId);

		var consumerName = nameof(InventoryReleaseRequestedConsumer).ToLowerInvariant();

		var already = await _db.ProcessedMessages.AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, context.CancellationToken);
		if (already)
		{
			_logger.LogInformation("Skipping already processed message {MessageId}, CorrelationId: {CorrelationId}", messageId, correlationId);
			return;
		}

		await using var tx = await _db.Database.BeginTransactionAsync(context.CancellationToken);
		try
		{
			await _handler.HandleInventoryReleaseRequestedAsync(context.Message, correlationId, messageId, context.CancellationToken);

			_db.ProcessedMessages.Add(new ProcessedMessage
			{
				MessageId = messageId,
				ConsumerName = consumerName,
				CorrelationId = correlationId,
				ProcessedAt = DateTime.UtcNow
			});

			await _db.SaveChangesAsync(context.CancellationToken);
			await tx.CommitAsync(context.CancellationToken);

			_logger.LogInformation(
				"Successfully processed InventoryReleaseRequested for OrderId: {OrderId}, CorrelationId: {CorrelationId}",
				message.OrderId,
				correlationId);
		}
		catch (Exception ex)
		{
			await tx.RollbackAsync(context.CancellationToken);
			_logger.LogError(ex,
				"Error processing message {MessageId}, CorrelationId: {CorrelationId}",
				messageId,
				correlationId);
			throw;
		}
	}
	}
}

