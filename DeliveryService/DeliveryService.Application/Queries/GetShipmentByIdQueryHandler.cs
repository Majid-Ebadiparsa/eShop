using DeliveryService.Application.Abstractions.Persistence;
using MediatR;
using SharedService.Caching.Abstractions;

namespace DeliveryService.Application.Queries
{
	public class GetShipmentByIdQueryHandler : IRequestHandler<GetShipmentByIdQuery, ShipmentReadModel?>
	{
		private readonly IShipmentReadRepository _readRepository;
		private readonly IRedisCacheClient _cache;
		private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

		public GetShipmentByIdQueryHandler(IShipmentReadRepository readRepository, IRedisCacheClient cache)
		{
			_readRepository = readRepository;
			_cache = cache;
		}

		public async Task<ShipmentReadModel?> Handle(GetShipmentByIdQuery request, CancellationToken cancellationToken)
		{
			var cacheKey = $"shipment:{request.ShipmentId}";
			var cached = await _cache.GetAsync<ShipmentReadModel>(cacheKey);
			if (cached != null) return cached;

			var shipment = await _readRepository.GetByIdAsync(request.ShipmentId, cancellationToken);

			if (shipment != null)
			{
				await _cache.SetAsync(cacheKey, shipment, CacheTtl);
			}

			return shipment;
		}
	}
}

