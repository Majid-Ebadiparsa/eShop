using MediatR;

namespace InvoiceService.Application.Invoices.Commands
{
	public record SubmitInvoiceCommand(
		string Description,
		DateTime DueDate,
		string Supplier,
		List<SubmitInvoiceLine> Lines
	) : IRequest<Guid>;

	public record SubmitInvoiceLine(string Description, double Price, int Quantity);
}
