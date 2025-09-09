using InvoiceService.Application.Abstractions.Read;
using InvoiceService.Application.Invoices.Queries.Models;
using MediatR;

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
