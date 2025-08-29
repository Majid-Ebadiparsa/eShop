using InvoiceService.Application.Abstractions;
using MassTransit;
using Shared.Contracts.Events;

namespace InvoiceService.Infrastructure.Messaging
{
	public class RabbitMqEventPublisher : IEventPublisher
	{
		private readonly IPublishEndpoint _publishEndpoint;

		public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint)
		{
			_publishEndpoint = publishEndpoint;
		}

		public async Task PublishInvoiceSubmittedAsync(InvoiceSubmitted @event, CancellationToken ct)
		{
			await _publishEndpoint.Publish(@event, ct);
		}
	}
}
