using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Shared.DTOs;
using SharedService.Caching.Abstractions;

namespace OrderService.Application.Queries
{
	public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IRedisCacheClient _cache;
		private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

		public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IRedisCacheClient cache)
		{
			_orderRepository = orderRepository;
			_cache = cache;
		}

		public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
		{
			var cacheKey = $"order:{request.OrderId}";
			var cached = await _cache.GetAsync<OrderDto>(cacheKey);
			if (cached != null) return cached;

			var dto = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

			await _cache.SetAsync(cacheKey, dto, CacheTtl);
			return dto;
		}
	}
}
