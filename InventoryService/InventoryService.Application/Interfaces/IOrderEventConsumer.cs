using SharedService.Contracts.Events;
using SharedService.Contracts.Events.Inventory;

namespace InventoryService.Application.Interfaces
{
	public interface IOrderEventConsumer
	{
		Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken);
		Task HandleInventoryReleaseRequestedAsync(InventoryReleaseRequested @event, CancellationToken cancellationToken);
	}
}
