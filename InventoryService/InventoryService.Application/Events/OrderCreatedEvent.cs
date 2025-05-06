namespace InventoryService.Application.Events
{
	public record OrderCreatedEvent(Guid OrderId, List<OrderItem> Items);
}
