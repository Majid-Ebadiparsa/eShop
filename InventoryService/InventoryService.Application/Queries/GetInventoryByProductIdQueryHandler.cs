using InventoryService.Application.Interfaces;
using InventoryService.Shared.DTOs;
using MediatR;
using SharedService.Caching.Abstractions;

namespace InventoryService.Application.Queries
{
	public class GetInventoryByProductIdQueryHandler : IRequestHandler<GetInventoryByProductIdQuery, InventoryDto?>
	{
		private readonly IInventoryReadRepository _readRepository;
		private readonly IRedisCacheClient _cache;
		private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

		public GetInventoryByProductIdQueryHandler(IInventoryReadRepository readRepository, IRedisCacheClient cache)
		{
			_readRepository = readRepository;
			_cache = cache;
		}

		public async Task<InventoryDto?> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
		{
			var cacheKey = $"inventory:{request.ProductId}";
			var cached = await _cache.GetAsync<InventoryDto>(cacheKey);
			if (cached != null) return cached;

			var dto = await _readRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

			if (dto != null)
			{
				await _cache.SetAsync(cacheKey, dto, CacheTtl);
			}

			return dto;
		}
	}
}
