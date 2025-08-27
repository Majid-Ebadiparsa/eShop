namespace Shared.Contracts.Events
{
	public record InvoiceLineItem
	(
		string Description, 
		double Price, 
		int Quantity
	);
}
