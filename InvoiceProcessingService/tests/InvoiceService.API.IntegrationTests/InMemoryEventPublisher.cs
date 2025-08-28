using InvoiceService.Application.Abstractions;
using Shared.Contracts.Events;

namespace InvoiceService.API.IntegrationTests
{
	public class InMemoryEventPublisher : IEventPublisher
	{
		public List<InvoiceSubmittedEvent> Published { get; } = new();

		public Task PublishInvoiceSubmittedAsync(InvoiceSubmittedEvent evt, CancellationToken ct)
		{
			Published.Add(evt);
			return Task.CompletedTask;
		}
	}
}
