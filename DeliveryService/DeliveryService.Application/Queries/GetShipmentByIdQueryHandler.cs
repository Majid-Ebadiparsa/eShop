using DeliveryService.Application.Abstractions.Persistence;
using MediatR;

namespace DeliveryService.Application.Queries
{
	public class GetShipmentByIdQueryHandler : IRequestHandler<GetShipmentByIdQuery, ShipmentReadModel?>
	{
		private readonly IShipmentReadRepository _readRepository;

		public GetShipmentByIdQueryHandler(IShipmentReadRepository readRepository)
		{
			_readRepository = readRepository;
		}

		public async Task<ShipmentReadModel?> Handle(GetShipmentByIdQuery request, CancellationToken cancellationToken)
		{
			return await _readRepository.GetByIdAsync(request.ShipmentId, cancellationToken);
		}
	}
}

