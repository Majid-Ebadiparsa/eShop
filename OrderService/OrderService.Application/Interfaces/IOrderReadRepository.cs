using OrderService.Shared.DTOs;

namespace OrderService.Application.Interfaces
{
	public interface IOrderReadRepository
	{
		Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
		Task<List<OrderDto>> GetAllAsync(CancellationToken cancellationToken);
		Task<List<OrderDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
		Task<List<OrderDto>> GetByStatusAsync(string status, CancellationToken cancellationToken);
	}
}

