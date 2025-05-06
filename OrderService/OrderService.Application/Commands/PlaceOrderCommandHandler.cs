using MassTransit;
using MediatR;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using OrderService.Domain.AggregatesModel;

namespace OrderService.Application.Commands
{
	public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IPublishEndpoint _publishEndpoint;

		public PlaceOrderCommandHandler(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint)
		{
			_orderRepository = orderRepository;
			_publishEndpoint = publishEndpoint;
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

			// Publish the OrderCreatedEvent after the order is saved
			var orderCreatedEvent = new OrderCreatedEvent(
				order.Id,
				order.Items.Select(i => new Events.OrderItem(i.ProductId, i.Quantity)).ToList()
			);

			await _publishEndpoint.Publish(orderCreatedEvent, cancellationToken);

			return order.Id;
		}
	}

}
