using System;

namespace Shared.Contracts.Invoices.Queries.Models
{
	// TODO: Should be better add this class in the Application layer of the InvoiceService
	// but for incompatibility .Net versions between projects, I add it here temporarily

	public class InvoiceDto
	{
		public string InvoiceNumber { get; set; } = null!;
		public string CustomerName { get; set; } = null!;
		public DateTime IssuedDate { get; set; }
		public double TotalAmount { get; set; }
	}
}
