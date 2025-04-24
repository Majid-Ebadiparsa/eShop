using OrderService.Application.Interfaces;
using OrderService.Domain.AggregatesModel;
using OrderService.Infrastructure.Repositories.EF;

namespace OrderService.Infrastructure.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		private readonly OrderDbContext _context;

		public OrderRepository(OrderDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(Order order)
		{
			await _context.Orders.AddAsync(order);
			await _context.SaveChangesAsync();
		}
	}
}
