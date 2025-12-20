using MassTransit;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events;
using SharedService.Contracts.Events.Inventory;

namespace OrderService.Infrastructure.Messaging
{
	public class RabbitMqEventPublisher : IEventPublisher
	{
		private readonly IPublishEndpoint _publishEndpoint;

		public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint)
		{
			_publishEndpoint = publishEndpoint;
		}

		public async Task PublishOrderCreatedAsync(Guid orderId, List<(Guid ProductId, int Quantity, decimal UnitPrice)> items)
		{
			var eventItems = items.Select(i => new OrderItem(i.ProductId, i.Quantity, i.UnitPrice)).ToList();
			var @event = new OrderCreatedEvent(orderId, eventItems);

			await _publishEndpoint.Publish(@event);
		}

		public async Task PublishInventoryReleaseRequestedAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items)
		{
			var eventItems = items.Select(i => new OrderItem(i.ProductId, i.Quantity, 0)).ToList();
			var @event = new InventoryReleaseRequested(orderId, eventItems, DateTime.UtcNow);

			await _publishEndpoint.Publish(@event);
		}
	}
}
