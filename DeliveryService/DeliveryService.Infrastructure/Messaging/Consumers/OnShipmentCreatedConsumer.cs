using DeliveryService.Application.Abstractions.Messaging;
using DeliveryService.Application.Abstractions.Persistence;
using DeliveryService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedService.Contracts.Events;

namespace DeliveryService.Infrastructure.Messaging.Consumers
{
	public sealed class OnShipmentCreatedConsumer : IConsumer<ShipmentCreated>
	{
		private readonly IShipmentRepository _repo;
		private readonly ICarrierClient _carrier;
		private readonly IUnitOfWork _uow;
		private readonly IEventPublisher _publisher;
		private readonly DeliveryDbContext _db;
		private readonly ILogger<OnShipmentCreatedConsumer> _logger;

		public OnShipmentCreatedConsumer(
			IShipmentRepository repo,
			ICarrierClient carrier,
			IUnitOfWork uow,
			IEventPublisher publisher,
			DeliveryDbContext db,
			ILogger<OnShipmentCreatedConsumer> logger)
		{
			_repo = repo;
			_carrier = carrier;
			_uow = uow;
			_publisher = publisher;
			_db = db;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<ShipmentCreated> context)
		{
			var ev = context.Message;
			var messageId = ev.MessageId;
			var correlationId = ev.CorrelationId;

			_logger.LogInformation(
				"Received ShipmentCreated for ShipmentId: {ShipmentId}, CorrelationId: {CorrelationId}, MessageId: {MessageId}",
				ev.ShipmentId,
				correlationId,
				messageId);

			var consumerName = nameof(OnShipmentCreatedConsumer).ToLowerInvariant();

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
				var shipment = await _repo.GetByIdAsync(ev.ShipmentId, context.CancellationToken);
				if (shipment == null)
				{
					_logger.LogWarning("Shipment not found for ShipmentId: {ShipmentId}", ev.ShipmentId);
					return;
				}

				try
				{
					var (carrier, tracking) = await _carrier.BookLabelAsync(shipment); // Mocked
					shipment.MarkBooked(carrier, tracking);

					await _publisher.AddAsync(new ShipmentBooked(
						shipment.Id,
						shipment.OrderId,
						carrier,
						tracking,
						Guid.NewGuid(),
						correlationId,
						messageId,
						DateTime.UtcNow), context.CancellationToken);
					await _uow.SaveChangesAsync(context.CancellationToken);
				}
				catch (Exception ex)
				{
					shipment.MarkBookingFailed(ex.Message);
					await _publisher.AddAsync(new ShipmentFailed(
						shipment.Id,
						shipment.OrderId,
						ex.Message,
						Guid.NewGuid(),
						correlationId,
						messageId,
						DateTime.UtcNow), context.CancellationToken);
					await _uow.SaveChangesAsync(context.CancellationToken);
				}

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
					"Successfully processed ShipmentCreated for ShipmentId: {ShipmentId}, CorrelationId: {CorrelationId}",
					ev.ShipmentId,
					correlationId);
			}
			catch (Exception ex)
			{
				await tx.RollbackAsync(context.CancellationToken);
				_logger.LogError(ex,
					"Error processing ShipmentCreated message {MessageId}, CorrelationId: {CorrelationId}",
					messageId,
					correlationId);
				throw;
			}
		}
	}
}
