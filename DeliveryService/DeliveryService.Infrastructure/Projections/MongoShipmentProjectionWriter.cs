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

        public Task UpsertShipmentAsync(
            Guid shipmentId, 
            Guid orderId, 
            string street, 
            string city, 
            string zip, 
            string country,
            DateTime createdAtUtc, 
            CancellationToken ct)
        {
            var update = Builders<ShipmentView>.Update
                .SetOnInsert(v => v.ShipmentId, shipmentId)
                .Set(v => v.OrderId, orderId)
                .Set(v => v.Street, street)
                .Set(v => v.City, city)
                .Set(v => v.Zip, zip)
                .Set(v => v.Country, country)
                .Set(v => v.CreatedAtUtc, createdAtUtc)
                .Set(v => v.UpdatedAtUtc, createdAtUtc)
                .Set(v => v.Status, "CREATED");

            return _col.UpdateOneAsync(v => v.ShipmentId == shipmentId, update, new UpdateOptions { IsUpsert = true }, ct);
        }

        public Task UpdateStatusAsync(Guid shipmentId, string status, DateTime updatedAtUtc, CancellationToken ct)
        {
            var update = Builders<ShipmentView>.Update
                .Set(v => v.Status, status)
                .Set(v => v.UpdatedAtUtc, updatedAtUtc);

            return _col.UpdateOneAsync(v => v.ShipmentId == shipmentId, update, new UpdateOptions { IsUpsert = false }, ct);
        }

        public Task UpdateCarrierInfoAsync(Guid shipmentId, string carrier, string trackingNumber, DateTime updatedAtUtc, CancellationToken ct)
        {
            var update = Builders<ShipmentView>.Update
                .Set(v => v.Carrier, carrier)
                .Set(v => v.TrackingNumber, trackingNumber)
                .Set(v => v.UpdatedAtUtc, updatedAtUtc);

            return _col.UpdateOneAsync(v => v.ShipmentId == shipmentId, update, new UpdateOptions { IsUpsert = false }, ct);
        }
    }
}
