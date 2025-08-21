using DeliveryService.Domain.AggregatesModel;

namespace DeliveryService.Application.Abstractions.Persistence
{
	public interface IShipmentRepository
	{
		Task AddAsync(Shipment shipment, CancellationToken ct = default);
		Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default);
		Task UpdateAsync(Shipment shipment, CancellationToken ct = default);
	}
}
