using OrderService.Domain.AggregatesModel;
using OrderService.Domain.SeedWork;
using OrderService.Shared.DTOs;

namespace OrderService.Application.Interfaces
{
	public interface IOrderRepository : IAggregateRoot
	{
		Task AddAsync(Order order, CancellationToken cancelationToken);

		Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cancelationToken);

		Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancelationToken);

		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}
