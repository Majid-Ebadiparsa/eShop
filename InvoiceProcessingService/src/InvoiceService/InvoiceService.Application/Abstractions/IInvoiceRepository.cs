using InvoiceService.Domain.Entities;
using InvoiceService.Domain.SeedWork;

namespace InvoiceService.Application.Abstractions
{
	public interface IInvoiceRepository : IAggregateRoot
	{
		Task AddAsync(Invoice invoice, CancellationToken cancelationToken);

		Task<int> CountAsync(CancellationToken cancelationToken);
	}
}
