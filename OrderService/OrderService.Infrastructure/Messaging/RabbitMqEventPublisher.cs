using MassTransit;
using OrderService.Application.Interfaces;
using SharedService.Contracts.Events;

namespace OrderService.Infrastructure.Messaging
{
	public class RabbitMqEventPublisher : IEventPublisher
	{
		private readonly IPublishEndpoint _publishEndpoint;

		public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint)
		{
			_publishEndpoint = publishEndpoint;
		}

		public async Task PublishOrderCreatedAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items)
		{
			var eventItems = items.Select(i => new OrderItem(i.ProductId, i.Quantity)).ToList();
			var @event = new OrderCreatedEvent(orderId, eventItems);

			await _publishEndpoint.Publish(@event);
		}
	}
}
