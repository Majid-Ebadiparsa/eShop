using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Shared.DTOs;

namespace OrderService.Application.Queries
{
	public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
	{
		private readonly IOrderRepository _orderRepository;

		public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
		{
			return await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);			
		}
	}
}
