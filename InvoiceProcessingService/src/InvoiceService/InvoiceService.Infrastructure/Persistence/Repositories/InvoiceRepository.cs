using InvoiceService.Application.Abstractions;
using InvoiceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Infrastructure.Persistence.Repositories
{
	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly ApplicationDbContext _context;

		public InvoiceRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Invoice invoice, CancellationToken cancelationToken)
		{
			await _context.Invoices.AddAsync(invoice, cancelationToken);
			await _context.SaveChangesAsync(cancelationToken); // Transactional with Outbox
		}

		public async Task<int> CountAsync(CancellationToken cancelationToken)
		{
			return await _context.Invoices.CountAsync(cancelationToken);
		}
	}
}
