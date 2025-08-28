namespace Shared.Contracts.Events
{
	public record InvoiceSubmittedEvent
	(
		Guid InvoiceId,
		string Description,
		DateTime DueDate,
		string Supplier,
		IReadOnlyList<InvoiceLineItem> Lines
	);
}