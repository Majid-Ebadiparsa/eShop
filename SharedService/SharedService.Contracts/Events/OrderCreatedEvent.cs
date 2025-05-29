namespace SharedService.Contracts.Events
{
	public record OrderCreatedEvent(Guid OrderId, List<OrderItem> Items);
}
