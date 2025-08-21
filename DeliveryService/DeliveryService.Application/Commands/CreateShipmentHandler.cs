using DeliveryService.Application.Abstractions;
using DeliveryService.Application.Repositories;
using DeliveryService.Domain.AggregatesModel;
using MediatR;
using SharedService.Contracts.Events;

namespace DeliveryService.Application.Commands
{
	public sealed class CreateShipmentHandler : IRequestHandler<CreateShipmentCommand, Guid>
	{
		private readonly IShipmentRepository _repo;
		private readonly IUnitOfWork _uow;
		private readonly IEventPublisher _publisher; // Connection to Outbox

		public CreateShipmentHandler(IShipmentRepository repo, IUnitOfWork uow, IEventPublisher publisher)
		{
			_repo = repo; _uow = uow; _publisher = publisher;
		}

		public async Task<Guid> Handle(CreateShipmentCommand cmd, CancellationToken ct)
		{
			var address = new Address(cmd.Address.Street, cmd.Address.City, cmd.Address.Zip, cmd.Address.Country);
			var items = cmd.Items.Select(i => new ShipmentItem(i.ProductId, i.Quantity));

			var shipment = Shipment.Create(cmd.OrderId, address, items);
			await _repo.AddAsync(shipment, ct);

			// Domain Event => Outbox row (ShipmentCreated)
			await _publisher.AddAsync(new ShipmentCreated(shipment.Id, shipment.OrderId, DateTime.UtcNow), ct);

			await _uow.SaveChangesAsync(ct);
			return shipment.Id;
		}
	}
}
