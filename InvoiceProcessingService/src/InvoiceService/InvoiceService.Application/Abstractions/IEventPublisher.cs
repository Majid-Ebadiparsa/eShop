using SharedService.Contracts.Events.Invoice;

namespace InvoiceService.Application.Abstractions
{
	public interface IEventPublisher
	{
		Task PublishInvoiceSubmittedAsync(
			Guid invoiceId,
			string description,
			DateTime dueDate,
			string supplier,
			IReadOnlyList<InvoiceLineItem> lines,
			Guid correlationId,
			CancellationToken ct);
	}
}
