using InventoryService.Application.Interfaces;
using InventoryService.Domain.AggregatesModel;
using MediatR;

namespace InventoryService.Application.Queries
{
	public class GetInventoryByProductIdQueryHandler : IRequestHandler<GetInventoryByProductIdQuery, InventoryItem?>
	{
		private readonly IInventoryRepository _repository;

		public GetInventoryByProductIdQueryHandler(IInventoryRepository repository)
		{
			_repository = repository;
		}

		public async Task<InventoryItem?> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
		{
			return await _repository.GetByProductIdAsync(request.ProductId, cancellationToken);
		}
	}
}
