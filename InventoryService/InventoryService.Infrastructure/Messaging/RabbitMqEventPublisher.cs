using InventoryService.Application.Interfaces;
using MassTransit;
using SharedService.Contracts.Events.Inventory;

namespace InventoryService.Infrastructure.Messaging
{
	public class RabbitMqEventPublisher : IEventPublisher
	{
		private readonly IPublishEndpoint _publishEndpoint;

		public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint)
		{
			_publishEndpoint = publishEndpoint;
		}

		public async Task PublishInventoryReservedAsync(Guid orderId, decimal totalAmount, string currency, CancellationToken cancellationToken)
		{
			var @event = new InventoryReserved(orderId, totalAmount, currency);
			await _publishEndpoint.Publish(@event, cancellationToken);
		}

		public async Task PublishInventoryReservationFailedAsync(Guid orderId, string reason, CancellationToken cancellationToken)
		{
			var @event = new InventoryReservationFailed(orderId, reason, DateTime.UtcNow);
			await _publishEndpoint.Publish(@event, cancellationToken);
		}
	}
}
