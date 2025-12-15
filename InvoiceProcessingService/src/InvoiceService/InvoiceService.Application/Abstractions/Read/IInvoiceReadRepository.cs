using InvoiceService.Application.Invoices.Queries.Models;

namespace InvoiceService.Application.Abstractions.Read
{
	public interface IInvoiceReadRepository
	{
		Task<List<InvoiceDto>> GetAllAsync();
		ValueTask<InvoiceDto> GetByIdAsync(string id);
		Task AddAsync(InvoiceDto invoice);
	}
}
