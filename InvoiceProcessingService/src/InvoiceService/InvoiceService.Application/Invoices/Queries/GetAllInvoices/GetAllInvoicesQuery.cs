using InvoiceService.Application.Invoices.Queries.Models;
using MediatR;

namespace InvoiceService.Application.Invoices.Queries.GetAllInvoices
{
	public class GetAllInvoicesQuery : IRequest<List<InvoiceDto>> { }
}
