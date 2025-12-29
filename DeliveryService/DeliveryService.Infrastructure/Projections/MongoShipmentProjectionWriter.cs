using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryService.Application.Abstractions;
using DeliveryService.Infrastructure.Projections;
using MongoDB.Driver;

namespace DeliveryService.Infrastructure.Projections
{
    public class MongoShipmentProjectionWriter : IShipmentProjectionWriter
    {
        private readonly IMongoCollection<ShipmentView> _col;

        public MongoShipmentProjectionWriter(IMongoClient client)
        {
            _col = client.GetDatabase("eshop_query").GetCollection<ShipmentView>("shipments");
        }

        public Task UpsertShipmentAsync(Guid shipmentId, Guid orderId, DateTime occurredAtUtc, CancellationToken ct)
        {
            var update = Builders<ShipmentView>.Update
                .SetOnInsert(v => v.ShipmentId, shipmentId)
                .Set(v => v.OrderId, orderId)
                .Set(v => v.OccurredAtUtc, occurredAtUtc)
                .Set(v => v.Status, "CREATED");

            return _col.UpdateOneAsync(v => v.ShipmentId == shipmentId, update, new UpdateOptions { IsUpsert = true }, ct);
        }
    }
}
