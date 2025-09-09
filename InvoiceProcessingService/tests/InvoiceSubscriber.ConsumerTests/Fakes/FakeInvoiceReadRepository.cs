using Shared.Contracts.Abstraction.Read;
using Shared.Contracts.Invoices.Queries.Models;
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

		public Task<List<InvoiceDto>> GetAllAsync() =>
				Task.FromResult(new List<InvoiceDto>());

		public Task<InvoiceDto?> GetByIdAsync(string id) =>
				Task.FromResult<InvoiceDto?>(null);
	}
}
