namespace OrderService.Application.Interfaces
{
	public interface IEventPublisher
	{
		Task PublishOrderCreatedAsync(Guid orderId, List<(Guid ProductId, int Quantity, decimal UnitPrice)> items);
		Task PublishInventoryReleaseRequestedAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items);
	}
}
