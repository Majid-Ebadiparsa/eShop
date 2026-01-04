using InventoryService.Application.Interfaces;
using InventoryService.Shared.DTOs;
using MediatR;

namespace InventoryService.Application.Queries
{
	public class GetInventoryByProductIdQueryHandler : IRequestHandler<GetInventoryByProductIdQuery, InventoryDto?>
	{
		private readonly IInventoryReadRepository _readRepository;

		public GetInventoryByProductIdQueryHandler(IInventoryReadRepository readRepository)
		{
			_readRepository = readRepository;
		}

		public async Task<InventoryDto?> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
		{
			return await _readRepository.GetByProductIdAsync(request.ProductId, cancellationToken);
		}
	}
}
