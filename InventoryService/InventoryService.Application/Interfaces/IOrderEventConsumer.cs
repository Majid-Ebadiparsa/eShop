using SharedService.Contracts.Events;
using SharedService.Contracts.Events.Inventory;

namespace InventoryService.Application.Interfaces;

public interface IOrderEventConsumer
{
	Task Handle(OrderCreatedEvent @event, Guid correlationId, Guid causationId, CancellationToken cancellationToken);
	Task HandleInventoryReleaseRequestedAsync(InventoryReleaseRequested @event, Guid correlationId, Guid causationId, CancellationToken cancellationToken);
}
