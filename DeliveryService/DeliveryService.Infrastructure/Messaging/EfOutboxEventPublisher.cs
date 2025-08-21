using DeliveryService.Application.Abstractions.Messaging;
using MassTransit;

namespace DeliveryService.Infrastructure.Messaging
{
	public sealed class EfOutboxEventPublisher : IEventPublisher
	{
		private readonly IPublishEndpoint _publish;
		public EfOutboxEventPublisher(IPublishEndpoint publish) => _publish = publish;

		public Task AddAsync<TEvent>(TEvent @event, CancellationToken ct = default)
				where TEvent : class
				=> _publish.Publish(@event, ct); // Atomic with MassTransit Outbox
	}
}
