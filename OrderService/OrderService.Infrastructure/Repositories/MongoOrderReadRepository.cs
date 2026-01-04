using MongoDB.Driver;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Projections;
using OrderService.Shared.DTOs;

namespace OrderService.Infrastructure.Repositories
{
	public class MongoOrderReadRepository : IOrderReadRepository
	{
		private readonly IMongoCollection<OrderView> _collection;

		public MongoOrderReadRepository(IMongoClient mongoClient)
		{
			var database = mongoClient.GetDatabase("eshop_query");
			_collection = database.GetCollection<OrderView>("orders");
		}

		public async Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
		{
			var view = await _collection.Find(v => v.OrderId == orderId)
				.FirstOrDefaultAsync(cancellationToken);

			return view == null ? null : MapToDto(view);
		}

		public async Task<List<OrderDto>> GetAllAsync(CancellationToken cancellationToken)
		{
			var views = await _collection.Find(_ => true)
				.ToListAsync(cancellationToken);

			return views.Select(MapToDto).ToList();
		}

		public async Task<List<OrderDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
		{
			var views = await _collection.Find(v => v.CustomerId == customerId)
				.ToListAsync(cancellationToken);

			return views.Select(MapToDto).ToList();
		}

		public async Task<List<OrderDto>> GetByStatusAsync(string status, CancellationToken cancellationToken)
		{
			var views = await _collection.Find(v => v.Status == status)
				.ToListAsync(cancellationToken);

			return views.Select(MapToDto).ToList();
		}

		private static OrderDto MapToDto(OrderView view)
		{
			var items = view.Items.Select(i => new OrderItemDto(
				i.ProductId,
				i.Quantity,
				i.UnitPrice
			)).ToList();

			return new OrderDto(
				Id: view.OrderId,
				CustomerId: view.CustomerId,
				Street: view.Street,
				City: view.City,
				PostalCode: view.PostalCode,
				OrderDate: view.CreatedAt,
				TotalAmount: view.Total,
				Items: items
			);
		}
	}
}

