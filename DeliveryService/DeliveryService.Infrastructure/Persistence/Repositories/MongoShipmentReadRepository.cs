using MongoDB.Driver;
using DeliveryService.Application.Abstractions.Persistence;
using DeliveryService.Application.Queries;
using DeliveryService.Infrastructure.Projections;

namespace DeliveryService.Infrastructure.Persistence.Repositories
{
	public class MongoShipmentReadRepository : IShipmentReadRepository
	{
		private readonly IMongoCollection<ShipmentView> _collection;

		public MongoShipmentReadRepository(IMongoClient mongoClient)
		{
			var database = mongoClient.GetDatabase("eshop_query");
			_collection = database.GetCollection<ShipmentView>("shipments");
		}

		public async Task<ShipmentReadModel?> GetByIdAsync(Guid shipmentId, CancellationToken ct = default)
		{
			var view = await _collection.Find(v => v.ShipmentId == shipmentId)
				.FirstOrDefaultAsync(ct);

			return view == null ? null : MapToReadModel(view);
		}

		public async Task<List<ShipmentReadModel>> GetAllAsync(CancellationToken ct = default)
		{
			var views = await _collection.Find(_ => true)
				.ToListAsync(ct);

			return views.Select(MapToReadModel).ToList();
		}

		public async Task<List<ShipmentReadModel>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
		{
			var views = await _collection.Find(v => v.OrderId == orderId)
				.ToListAsync(ct);

			return views.Select(MapToReadModel).ToList();
		}

		public async Task<List<ShipmentReadModel>> GetByStatusAsync(string status, CancellationToken ct = default)
		{
			var views = await _collection.Find(v => v.Status == status)
				.ToListAsync(ct);

			return views.Select(MapToReadModel).ToList();
		}

		private static ShipmentReadModel MapToReadModel(ShipmentView view)
		{
			return new ShipmentReadModel(
				ShipmentId: view.ShipmentId,
				OrderId: view.OrderId,
				Status: view.Status,
				Carrier: view.Carrier,
				TrackingNumber: view.TrackingNumber,
				Street: view.Street,
				City: view.City,
				Zip: view.Zip,
				Country: view.Country,
				CreatedAtUtc: view.CreatedAtUtc
			);
		}
	}
}

