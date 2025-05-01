using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.AggregatesModel;
using OrderService.Infrastructure.Repositories.EF;
using OrderService.Shared.DTOs;

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

		public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancelationToken)
		{
			var order = await _context.Orders
			.Include(o => o.Items)
			.FirstOrDefaultAsync(o => o.Id == id, cancelationToken);

			if (order is null)
				return null;

			var items = order.Items.Select(i => new OrderItemDto(i.ProductId, i.Quantity, i.UnitPrice)).ToList();

			return new OrderDto(
				order.Id,
				order.CustomerId,
				order.ShippingAddress.Street,
				order.ShippingAddress.City,
				order.ShippingAddress.PostalCode,
				order.OrderDate,
				order.TotalAmount,
				items
			);
		}
	}
}
