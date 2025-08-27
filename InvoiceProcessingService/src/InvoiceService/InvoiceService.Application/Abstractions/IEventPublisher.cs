using Shared.Contracts.Events;

namespace InvoiceService.Application.Abstractions
{
	public interface IEventPublisher
	{
		Task PublishInvoiceSubmittedAsync(InvoiceSubmitted invoiceSubmitted, CancellationToken ct);
	}
}
