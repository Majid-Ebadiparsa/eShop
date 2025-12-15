using InvoiceService.Application.Abstractions.Read;
using InvoiceService.Application.Invoices.Queries.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceSubscriber.ConsumerTests.Fakes
{
	public sealed class FakeInvoiceReadRepository : IInvoiceReadRepository
	{
		public bool AddCalled { get; private set; } = false;
		public InvoiceDto? LastAddedInvoice { get; private set; }

		public Task AddAsync(InvoiceDto invoice)
		{
			AddCalled = true;
			LastAddedInvoice = invoice;
			return Task.CompletedTask;
		}

		public async Task<List<InvoiceDto>> GetAllAsync() =>
				await Task.FromResult(new List<InvoiceDto>());

		public async ValueTask<InvoiceDto> GetByIdAsync(string id)
		{
			return await ValueTask.FromResult<InvoiceDto>(null!);
		}
	}
}
