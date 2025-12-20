using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Domain.AggregatesModel;
using SharedService.Caching;
using SharedService.Caching.Abstractions;
using SharedService.Contracts.Events;

namespace OrderService.Application.Commands
{
	public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IEventPublisher _eventPublisher;
    private readonly IRedisCacheClient _cache;

    public PlaceOrderCommandHandler(IOrderRepository orderRepository, IEventPublisher eventPublisher, IRedisCacheClient cache)
		{
			_orderRepository = orderRepository;
			_eventPublisher = eventPublisher;
      _cache = cache;
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
				order.Items.Select(i => new SharedService.Contracts.Events.OrderItem(i.ProductId, i.Quantity, i.UnitPrice)).ToList()
			);

			await _eventPublisher.PublishOrderCreatedAsync(order.Id, [.. order.Items.Select(i => (i.ProductId, i.Quantity, i.UnitPrice))]);

      // invalidate cache (in case order is read soon after creation)
      await _cache.RemoveAsync($"order:{order.Id}");

      return order.Id;
		}
	}

}
