using MediatR;
using Shared.Contracts.Abstraction.Read;
using Shared.Contracts.Invoices.Queries.Models;

namespace InvoiceService.Application.Invoices.Queries.GetAllInvoices
{
	public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, List<InvoiceDto>>
	{
		private readonly IInvoiceReadRepository _readRepository;

		public GetAllInvoicesQueryHandler(IInvoiceReadRepository readRepository)
		{
			_readRepository = readRepository;
		}

		public async Task<List<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
		{
			return await _readRepository.GetAllAsync();
		}
	}
}
