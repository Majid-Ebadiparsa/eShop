using Shared.Contracts.Events;

namespace InvoiceService.Application.Abstractions
{
	public interface IEventPublisher
	{
		Task PublishInvoiceSubmittedAsync(InvoiceSubmittedEvent @event, CancellationToken ct);
	}
}
