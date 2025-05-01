using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Domain.AggregatesModel;

namespace OrderService.Application.Commands
{
	public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
	{
		private readonly IOrderRepository _orderRepository;

		public PlaceOrderCommandHandler(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
		{
			var address = new Address(request.Street, request.City, request.PostalCode);
			var order = new Order(request.CustomerId, address);

			foreach (var item in request.Items)
			{
				order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
			}

			await _orderRepository.AddAsync(order, cancellationToken);
			return order.Id;
		}
	}

}
