namespace InvoiceService.API.DTOs
{
	public record InvoiceLineDto(string Description, double Price, int Quantity);
}
