using SharedService.Contracts.Events;

namespace InventoryService.Application.Interfaces
{
	public interface IOrderEventConsumer
	{
		Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken);
	}
}
