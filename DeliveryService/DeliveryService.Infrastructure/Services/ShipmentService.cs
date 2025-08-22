using DeliveryService.Application.Abstractions.Messaging;
using DeliveryService.Application.Abstractions.Persistence;
using DeliveryService.Application.Abstractions.Services;
using DeliveryService.Domain.SeedWork;
using SharedService.Contracts.Events;

namespace DeliveryService.Infrastructure.Services
{
	public sealed class ShipmentService : IShipmentService
	{
		private readonly IShipmentRepository _repo;
		private readonly IUnitOfWork _uow;
		private readonly IEventPublisher _publisher;

		public ShipmentService(
				IShipmentRepository repo,
				IUnitOfWork uow,
				IEventPublisher publisher)
		{
			_repo = repo;
			_uow = uow;
			_publisher = publisher;
		}

		public async Task MarkDispatchedAsync(Guid shipmentId, CancellationToken ct = default)
		{
			var shipment = await _repo.GetByIdAsync(shipmentId, ct)
										?? throw new KeyNotFoundException("Shipment not found.");

			// Idempotency: If already Dispatched, do nothing
			if (shipment.Status == ShipmentStatus.Dispatched || shipment.Status == ShipmentStatus.InTransit)
				return;

			shipment.MarkDispatched();

			// For Outbox pattern, we publish the event after marking the shipment as dispatched
			await _publisher.AddAsync(new ShipmentDispatched(
					shipment.Id, shipment.OrderId, shipment.Carrier ?? "N/A",
					shipment.TrackingNumber ?? string.Empty, DateTime.UtcNow), ct);

			await _uow.SaveChangesAsync(ct);
		}

		public async Task MarkDeliveredAsync(Guid shipmentId, CancellationToken ct = default)
		{
			var shipment = await _repo.GetByIdAsync(shipmentId, ct)
										?? throw new KeyNotFoundException("Shipment not found.");

			// Idempotency: If already Delivered, do nothing
			if (shipment.Status == ShipmentStatus.Delivered)
				return;

			shipment.MarkDelivered();

			await _publisher.AddAsync(new ShipmentDelivered(
					shipment.Id, shipment.OrderId, shipment.Carrier ?? "N/A",
					shipment.TrackingNumber ?? string.Empty, DateTime.UtcNow), ct);

			await _uow.SaveChangesAsync(ct);
		}
	}
}
