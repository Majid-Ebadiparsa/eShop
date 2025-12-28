using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events.Payment;
using OrderService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Messaging
{
	public class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorized>
	{
		private readonly IOrderEventConsumer _handler;
		private readonly ILogger<PaymentAuthorizedConsumer> _logger;
		private readonly OrderDbContext _db;

		public PaymentAuthorizedConsumer(IOrderEventConsumer handler, ILogger<PaymentAuthorizedConsumer> logger, OrderDbContext db)
		{
			_handler = handler;
			_logger = logger;
			_db = db;
		}

		public async Task Consume(ConsumeContext<PaymentAuthorized> context)
		{
			_logger.LogInformation("Received PaymentAuthorized for OrderId: {OrderId}", context.Message.OrderId);
			var messageId = context.MessageId ?? Guid.NewGuid();
			var consumerName = nameof(PaymentAuthorizedConsumer).ToLowerInvariant();

			var already = await _db.ProcessedMessages.AnyAsync(p => p.MessageId == messageId && p.ConsumerName == consumerName, context.CancellationToken);
			if (already)
			{
				_logger.LogInformation("Skipping already processed message {MessageId}", messageId);
				return;
			}

			await using var tx = await _db.Database.BeginTransactionAsync(context.CancellationToken);
			try
			{
				await _handler.HandlePaymentAuthorizedAsync(context.Message, context.CancellationToken);

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

