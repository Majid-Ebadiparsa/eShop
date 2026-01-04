using DeliveryService.Application.Commands;
using DeliveryService.Application.DTOs;
using DeliveryService.Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events;

namespace DeliveryService.Infrastructure.Messaging.Consumers
{
	public sealed class OrderReadyToShipConsumer : IConsumer<OrderReadyToShip>
	{
		private readonly IMediator _mediator;
		private readonly DeliveryDbContext _db;
		private readonly ILogger<OrderReadyToShipConsumer> _logger;

		public OrderReadyToShipConsumer(IMediator mediator, DeliveryDbContext db, ILogger<OrderReadyToShipConsumer> logger)
		{
			_mediator = mediator;
			_db = db;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<OrderReadyToShip> context)
		{
			var msg = context.Message;
			var messageId = msg.MessageId;
			var correlationId = msg.CorrelationId;

			_logger.LogInformation(
				"Received OrderReadyToShip for OrderId: {OrderId}, CorrelationId: {CorrelationId}, MessageId: {MessageId}",
				msg.OrderId,
				correlationId,
				messageId);

			var consumerName = nameof(OrderReadyToShipConsumer).ToLowerInvariant();

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
				var cmd = new CreateShipmentCommand(
					msg.OrderId,
					new AddressDto(msg.Address.Street, msg.Address.City, msg.Address.Zip, msg.Address.Country),
					msg.Items.Select(i => new ShipmentItemDto(i.ProductId, i.Quantity)).ToList()
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
					"Error processing OrderReadyToShip message {MessageId}, CorrelationId: {CorrelationId}",
					messageId,
					correlationId);
				throw;
			}
		}
	}
}
