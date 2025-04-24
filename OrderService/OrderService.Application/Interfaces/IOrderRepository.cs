using OrderService.Domain.AggregatesModel;
using OrderService.Domain.SeedWork;

namespace OrderService.Application.Interfaces
{
	public interface IOrderRepository: IAggregateRoot
	{
		Task AddAsync(Order order);
	}
}
