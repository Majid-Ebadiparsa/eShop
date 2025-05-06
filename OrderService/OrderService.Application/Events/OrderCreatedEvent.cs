using OrderService.Domain.AggregatesModel;

namespace OrderService.Application.Events
{
	public record OrderCreatedEvent(Guid OrderId, List<OrderItem> Items);
}
