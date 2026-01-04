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
            // When an order is created, inventory is decreased in SQL (Available goes down)
            // In the read model, we track both Reserved (goes up) and Available (goes down)
            foreach (var it in items)
            {
                var filter = Builders<InventoryView>.Filter.Eq(v => v.ProductId, it.ProductId);
                var update = Builders<InventoryView>.Update
                    .Inc(v => v.Reserved, it.Quantity)
                    .Inc(v => v.Available, -it.Quantity)
                    .Set(v => v.LastUpdated, DateTime.UtcNow)
                    .SetOnInsert(v => v.ProductId, it.ProductId);

                await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
            }
        }

        public async Task ReleaseReservedInventoryAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items, CancellationToken ct)
        {
            // When inventory is released, SQL increases it back (Available goes up)
            // In the read model, we track both Reserved (goes down) and Available (goes up)
            foreach (var it in items)
            {
                var filter = Builders<InventoryView>.Filter.Eq(v => v.ProductId, it.ProductId);
                var update = Builders<InventoryView>.Update
                    .Inc(v => v.Reserved, -it.Quantity)
                    .Inc(v => v.Available, it.Quantity)
                    .Set(v => v.LastUpdated, DateTime.UtcNow);

                await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false }, ct);
            }
        }

        public async Task SyncInventoryFromSourceAsync(Guid productId, int availableQuantity, CancellationToken ct)
        {
            // This method syncs the Available quantity from the source (SQL) to the projection
            // Useful for initial seeding or repairing inconsistencies
            var filter = Builders<InventoryView>.Filter.Eq(v => v.ProductId, productId);
            var update = Builders<InventoryView>.Update
                .Set(v => v.Available, availableQuantity)
                .Set(v => v.LastUpdated, DateTime.UtcNow)
                .SetOnInsert(v => v.ProductId, productId)
                .SetOnInsert(v => v.Reserved, 0);

            await _col.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
        }
    }
}
