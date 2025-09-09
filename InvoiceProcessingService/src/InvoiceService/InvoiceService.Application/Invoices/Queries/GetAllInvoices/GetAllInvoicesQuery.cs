using MediatR;
using Shared.Contracts.Invoices.Queries.Models;

namespace InvoiceService.Application.Invoices.Queries.GetAllInvoices
{
	public class GetAllInvoicesQuery : IRequest<List<InvoiceDto>> { }
}
