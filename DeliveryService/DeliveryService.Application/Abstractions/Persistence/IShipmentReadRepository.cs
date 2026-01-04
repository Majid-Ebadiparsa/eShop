using DeliveryService.Application.Queries;

namespace DeliveryService.Application.Abstractions.Persistence
{
	public interface IShipmentReadRepository
	{
		Task<ShipmentReadModel?> GetByIdAsync(Guid shipmentId, CancellationToken ct = default);
		Task<List<ShipmentReadModel>> GetAllAsync(CancellationToken ct = default);
		Task<List<ShipmentReadModel>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
		Task<List<ShipmentReadModel>> GetByStatusAsync(string status, CancellationToken ct = default);
	}
}

