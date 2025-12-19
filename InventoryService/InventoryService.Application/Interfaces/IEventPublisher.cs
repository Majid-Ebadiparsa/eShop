using SharedService.Contracts.Events.Inventory;

namespace InventoryService.Application.Interfaces
{
	public interface IEventPublisher
	{
		Task PublishInventoryReservedAsync(Guid orderId, decimal totalAmount, string currency, CancellationToken cancellationToken);
		Task PublishInventoryReservationFailedAsync(Guid orderId, string reason, CancellationToken cancellationToken);
	}
}
