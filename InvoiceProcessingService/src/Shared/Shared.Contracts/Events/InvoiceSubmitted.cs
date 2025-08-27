namespace Shared.Contracts.Events
{
	public record InvoiceSubmitted
	(
		Guid InvoiceId,
		string Description,
		DateTime DueDate,
		string Supplier,
		IReadOnlyList<InvoiceLineItem> Lines
	);
}