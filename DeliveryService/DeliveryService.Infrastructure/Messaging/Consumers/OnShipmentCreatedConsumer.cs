using DeliveryService.Application.Abstractions;
using DeliveryService.Application.Repositories;
using MassTransit;
using SharedService.Contracts.Events;

namespace DeliveryService.Infrastructure.Messaging.Consumers
{
	public sealed class OnShipmentCreatedConsumer : IConsumer<ShipmentCreated>
	{
		private readonly IShipmentRepository _repo;
		private readonly ICarrierClient _carrier;
		private readonly IUnitOfWork _uow;
		private readonly IEventPublisher _publisher;

		public OnShipmentCreatedConsumer(IShipmentRepository repo, ICarrierClient carrier, IUnitOfWork uow, IEventPublisher publisher)
		{
			_repo = repo; _carrier = carrier; _uow = uow; _publisher = publisher;
		}

		public async Task Consume(ConsumeContext<ShipmentCreated> context)
		{
			var ev = context.Message;
			var shipment = await _repo.GetByIdAsync(ev.ShipmentId, context.CancellationToken);
			if (shipment == null) return; // idempotent

			try
			{
				var (carrier, tracking) = await _carrier.BookLabelAsync(shipment); // Mocked
				shipment.MarkBooked(carrier, tracking);

				await _publisher.AddAsync(new ShipmentBooked(shipment.Id, shipment.OrderId, carrier, tracking, DateTime.UtcNow), context.CancellationToken);
				await _uow.SaveChangesAsync(context.CancellationToken);
			}
			catch (Exception ex)
			{
				shipment.MarkBookingFailed(ex.Message);
				await _publisher.AddAsync(new ShipmentFailed(shipment.Id, shipment.OrderId, ex.Message, DateTime.UtcNow), context.CancellationToken);
				await _uow.SaveChangesAsync(context.CancellationToken);
			}
		}
	}
}
