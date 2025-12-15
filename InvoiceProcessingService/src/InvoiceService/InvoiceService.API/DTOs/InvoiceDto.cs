namespace InvoiceService.API.DTOs
{
	public record InvoiceDto(string Description, DateTime DueDate, string Supplier, List<InvoiceLineDto> Lines);
}
