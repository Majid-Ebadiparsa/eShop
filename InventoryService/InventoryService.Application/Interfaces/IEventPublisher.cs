using SharedService.Contracts.Events.Inventory;

namespace InventoryService.Application.Interfaces;

public interface IEventPublisher
{
	Task PublishInventoryReservedAsync(
		Guid orderId,
		decimal totalAmount,
		string currency,
		Guid correlationId,
		Guid causationId,
		CancellationToken cancellationToken);

	Task PublishInventoryReservationFailedAsync(
		Guid orderId,
		string reason,
		Guid correlationId,
		Guid causationId,
		CancellationToken cancellationToken);
}
