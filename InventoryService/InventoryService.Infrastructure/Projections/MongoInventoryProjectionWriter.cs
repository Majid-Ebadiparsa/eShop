using InventoryService.Application.Interfaces;
using MongoDB.Driver;

namespace InventoryService.Infrastructure.Projections
{
    public class MongoInventoryProjectionWriter : IInventoryProjectionWriter
    {
        private readonly IMongoCollection<InventoryView> _col;

        public MongoInventoryProjectionWriter(IMongoClient client)
        {
            _col = client.GetDatabase("eshop_query").GetCollection<InventoryView>("inventory");
        }

        public async Task UpsertInventoryForOrderAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items, CancellationToken ct)
        {
            // For minimal projection, we will increment Reserved for each product
            foreach (var it in items)
            {
                var filter = Builders<InventoryView>.Filter.Eq(v => v.ProductId, it.ProductId);
                var update = Builders<InventoryView>.Update
                    .Inc(v => v.Reserved, it.Quantity)
                    .Set(v => v.LastUpdated, DateTime.UtcNow)
                    .SetOnInsert(v => v.ProductId, it.ProductId)
                    .SetOnInsert(v => v.Available, 0);

                await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
            }
        }
    }
}
