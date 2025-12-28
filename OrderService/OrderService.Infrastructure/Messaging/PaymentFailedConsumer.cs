using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Payment;
using OrderService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Messaging
{
	public class PaymentFailedConsumer : IConsumer<PaymentFailed>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<PaymentFailedConsumer> _logger;
		private readonly OrderDbContext _db;

		public PaymentFailedConsumer(IOrderEventConsumer handler, ILogger<PaymentFailedConsumer> logger, OrderDbContext db)
		{
			_handler = handler;
			_logger = logger;
			_db = db;
		}

		public async Task Consume(ConsumeContext<PaymentFailed> context)
		{
			_logger.LogInformation("Received PaymentFailed for OrderId: {OrderId}", context.Message.OrderId);
			var messageId = context.MessageId ?? Guid.NewGuid();
			var consumerName = nameof(PaymentFailedConsumer).ToLowerInvariant();

			var already = await _db.ProcessedMessages.AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, context.CancellationToken);
			if (already)
			{
				_logger.LogInformation("Skipping already processed message {MessageId}", messageId);
				return;
			}

			await using var tx = await _db.Database.BeginTransactionAsync(context.CancellationToken);
			try
			{
				await _handler.HandlePaymentFailedAsync(context.Message, context.CancellationToken);

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

