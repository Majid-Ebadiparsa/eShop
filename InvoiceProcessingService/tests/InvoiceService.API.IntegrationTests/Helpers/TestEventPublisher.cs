using InvoiceService.Application.Abstractions;
using MassTransit;
using SharedService.Contracts.Events.Invoice;

namespace InvoiceService.API.IntegrationTests.Helpers
{
	/// <summary>
	/// Test implementation of IEventPublisher that publishes directly without outbox
	/// </summary>
	public class TestEventPublisher : IEventPublisher
	{
		private readonly IPublishEndpoint _publish;

		public TestEventPublisher(IPublishEndpoint publish)
		{
			_publish = publish;
		}

		public Task PublishInvoiceSubmittedAsync(
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

			// Publish directly without outbox
			return _publish.Publish(@event, ct);
		}
	}
}

