using MongoDB.Driver;
using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Projections;
using InventoryService.Shared.DTOs;

namespace InventoryService.Infrastructure.Repositories
{
	public class MongoInventoryReadRepository : IInventoryReadRepository
	{
		private readonly IMongoCollection<InventoryView> _collection;

		public MongoInventoryReadRepository(IMongoClient mongoClient)
		{
			var database = mongoClient.GetDatabase("eshop_query");
			_collection = database.GetCollection<InventoryView>("inventory");
		}

		public async Task<InventoryDto?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
		{
			var view = await _collection.Find(v => v.ProductId == productId)
				.FirstOrDefaultAsync(cancellationToken);

			return view == null ? null : MapToDto(view);
		}

		public async Task<List<InventoryDto>> GetAllAsync(CancellationToken cancellationToken)
		{
			var views = await _collection.Find(_ => true)
				.ToListAsync(cancellationToken);

			return views.Select(MapToDto).ToList();
		}

		public async Task<List<InventoryDto>> GetLowStockAsync(int threshold, CancellationToken cancellationToken)
		{
			var views = await _collection.Find(v => v.Available < threshold)
				.ToListAsync(cancellationToken);

			return views.Select(MapToDto).ToList();
		}

		private static InventoryDto MapToDto(InventoryView view)
		{
			return new InventoryDto(
				ProductId: view.ProductId,
				Available: view.Available,
				Reserved: view.Reserved,
				LastUpdated: view.LastUpdated
			);
		}
	}
}

