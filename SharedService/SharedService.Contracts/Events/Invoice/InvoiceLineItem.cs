namespace SharedService.Contracts.Events.Invoice
{
	public record InvoiceLineItem(
		string Description,
		decimal Price,
		int Quantity);
}

