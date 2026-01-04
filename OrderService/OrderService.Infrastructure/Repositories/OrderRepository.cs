using Microsoft.EntityFrameworkCore;
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

		public async Task AddAsync(Order order, CancellationToken cancelationToken)
		{
			await _context.Orders.AddAsync(order);
			await _context.SaveChangesAsync(cancelationToken);
		}

		public async Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken cancelationToken)
		{
			return await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == id, cancelationToken);
		}

		public async Task SaveChangesAsync(CancellationToken cancellationToken)
		{
			await _context.SaveChangesAsync(cancellationToken);
		}
	}
}
