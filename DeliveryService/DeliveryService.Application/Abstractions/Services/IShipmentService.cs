namespace DeliveryService.Application.Abstractions.Services
{
	public interface IShipmentService
	{
		Task MarkDispatchedAsync(Guid shipmentId, CancellationToken ct = default);
		Task MarkDeliveredAsync(Guid shipmentId, CancellationToken ct = default);
	}
}
