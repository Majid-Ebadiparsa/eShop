using InvoiceService.Infrastructure.Mongo.Context;
using InvoiceService.Infrastructure.Mongo.Models;
using MongoDB.Driver;
using Shared.Contracts.Abstraction.Read;
using Shared.Contracts.Invoices.Queries.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceService.Infrastructure.Mongo.Repositories
{
	public class InvoiceReadRepository : IInvoiceReadRepository
	{
		private readonly MongoContext _context;

		public InvoiceReadRepository(MongoContext context)
		{
			_context = context;
		}

		public async Task<List<InvoiceDto>> GetAllAsync()
		{
			var result = await _context.Invoices.Find(_ => true).ToListAsync();
			return result.Select(x => new InvoiceDto
			{
				InvoiceNumber = x.InvoiceNumber,
				CustomerName = x.CustomerName,
				IssuedDate = x.IssuedDate,
				TotalAmount = x.TotalAmount
			}).ToList();
		}

		public async Task<InvoiceDto?> GetByIdAsync(string id)
		{
			var item = await _context.Invoices.Find(x => x.Id == id).FirstOrDefaultAsync();
			return item is null ? null : new InvoiceDto
			{
				InvoiceNumber = item.InvoiceNumber,
				CustomerName = item.CustomerName,
				IssuedDate = item.IssuedDate,
				TotalAmount = item.TotalAmount
			};
		}

		public async Task AddAsync(InvoiceDto invoice)
		{
			var invoiceReadModel = new InvoiceReadModel
			{
				InvoiceNumber = invoice.InvoiceNumber,
				CustomerName = invoice.CustomerName,
				IssuedDate = invoice.IssuedDate,
				TotalAmount = invoice.TotalAmount
			};

			await _context.Invoices.InsertOneAsync(invoiceReadModel);
		}
	}
}
