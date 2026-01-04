using DeliveryService.Application.Abstractions.Services;
using DeliveryService.Application.Commands;
using DeliveryService.Application.DTOs;
using DeliveryService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events.Payment;

namespace DeliveryService.Infrastructure.Messaging.Consumers
{
	public sealed class PaymentCapturedConsumer : IConsumer<PaymentCaptured>
	{
		private readonly IMediator _mediator;
		private readonly IOrderServiceClient _orderServiceClient;
		private readonly ILogger<PaymentCapturedConsumer> _logger;
		private readonly DeliveryDbContext _db;

		public PaymentCapturedConsumer(
			IMediator mediator,
			IOrderServiceClient orderServiceClient,
			ILogger<PaymentCapturedConsumer> logger,
			DeliveryDbContext db)
		{
			_mediator = mediator;
			_orderServiceClient = orderServiceClient;
			_logger = logger;
			_db = db;
		}

		public async Task Consume(ConsumeContext<PaymentCaptured> context)
		{
			var msg = context.Message;
			var messageId = msg.MessageId;
			var correlationId = msg.CorrelationId;

			_logger.LogInformation(
				"Received PaymentCaptured for OrderId: {OrderId}, CorrelationId: {CorrelationId}, MessageId: {MessageId}",
				msg.OrderId,
				correlationId,
				messageId);

			var consumerName = nameof(PaymentCapturedConsumer).ToLowerInvariant();

			var already = await _db.ProcessedMessages.AnyAsync(
				p => p.MessageId == messageId && p.ConsumerName == consumerName,
				context.CancellationToken);

			if (already)
			{
				_logger.LogInformation("Skipping already processed message {MessageId}, CorrelationId: {CorrelationId}", messageId, correlationId);
				return;
			}

			await using var tx = await _db.Database.BeginTransactionAsync(context.CancellationToken);
			try
			{
				var orderDetails = await _orderServiceClient.GetOrderDetailsAsync(msg.OrderId, context.CancellationToken);
				if (orderDetails == null)
				{
					_logger.LogWarning("Order details not found for OrderId: {OrderId}", msg.OrderId);
					return;
				}

				var cmd = new CreateShipmentCommand(
					msg.OrderId,
					new AddressDto(orderDetails.Street, orderDetails.City, orderDetails.PostalCode, "US"),
					orderDetails.Items.Select(i => new ShipmentItemDto(i.ProductId, i.Quantity)).ToList()
				);

				var shipmentId = await _mediator.Send(cmd, context.CancellationToken);

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
					"Successfully created shipment {ShipmentId} for OrderId: {OrderId}, CorrelationId: {CorrelationId}",
					shipmentId,
					msg.OrderId,
					correlationId);
			}
			catch (Exception ex)
			{
				await tx.RollbackAsync(context.CancellationToken);
				_logger.LogError(ex,
					"Error processing PaymentCaptured message {MessageId}, CorrelationId: {CorrelationId}",
					messageId,
					correlationId);
				throw;
			}
		}
	}
}

