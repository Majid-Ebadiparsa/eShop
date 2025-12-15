namespace InvoiceService.Application.Invoices.Queries.Models
{
	public class InvoiceDto
	{
		public string InvoiceNumber { get; set; } = null!;
		public string CustomerName { get; set; } = null!;
		public DateTime IssuedDate { get; set; }
		public double TotalAmount { get; set; }
	}
}
