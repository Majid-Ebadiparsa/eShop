using MassTransit;
using PaymentService.Application.Abstractions;

namespace PaymentService.Infrastructure.Messaging
{
	public class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
	{
		private readonly IPublishEndpoint _publish;
		public MassTransitIntegrationEventPublisher(IPublishEndpoint publish) => _publish = publish;
		public Task PublishAsync<T>(T @event, CancellationToken ct) => _publish.Publish(@event, ct);
	}
}
