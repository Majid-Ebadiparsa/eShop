using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Shared.DTOs;
using SharedService.Caching.Abstractions;

namespace OrderService.Application.Queries
{
	public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
	{
		private readonly IOrderReadRepository _orderReadRepository;
		private readonly IRedisCacheClient _cache;
		private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

		public GetOrderByIdQueryHandler(IOrderReadRepository orderReadRepository, IRedisCacheClient cache)
		{
			_orderReadRepository = orderReadRepository;
			_cache = cache;
		}

		public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
		{
			var cacheKey = $"order:{request.OrderId}";
			var cached = await _cache.GetAsync<OrderDto>(cacheKey);
			if (cached != null) return cached;

			var dto = await _orderReadRepository.GetByIdAsync(request.OrderId, cancellationToken);

			if (dto != null)
			{
				await _cache.SetAsync(cacheKey, dto, CacheTtl);
			}

			return dto;
		}
	}
}
