using InvoiceService.Application.Abstractions;
using MassTransit;
using SharedService.Contracts.Events.Invoice;

namespace InvoiceService.Infrastructure.Messaging
{
	public class RabbitMqEventPublisher : IEventPublisher
	{
		private readonly IPublishEndpoint _publishEndpoint;

		public RabbitMqEventPublisher(IPublishEndpoint publishEndpoint)
		{
			_publishEndpoint = publishEndpoint;
		}

		public async Task PublishInvoiceSubmittedAsync(
			Guid invoiceId,
			string description,
			DateTime dueDate,
			string supplier,
			IReadOnlyList<InvoiceLineItem> lines,
			Guid correlationId,
			CancellationToken ct)
		{
			var @event = new InvoiceSubmitted(
				InvoiceId: invoiceId,
				Description: description,
				DueDate: dueDate,
				Supplier: supplier,
				Lines: lines,
				MessageId: Guid.NewGuid(),
				CorrelationId: correlationId,
				CausationId: null,
				OccurredAtUtc: DateTime.UtcNow
			);

			await _publishEndpoint.Publish(@event, ct);
		}
	}
}
