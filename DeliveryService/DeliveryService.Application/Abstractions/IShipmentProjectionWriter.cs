using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeliveryService.Application.Abstractions
{
    public interface IShipmentProjectionWriter
    {
        Task UpsertShipmentAsync(
            Guid shipmentId, 
            Guid orderId, 
            string street, 
            string city, 
            string zip, 
            string country,
            DateTime createdAtUtc, 
            CancellationToken ct);
        
        Task UpdateStatusAsync(Guid shipmentId, string status, DateTime updatedAtUtc, CancellationToken ct);
        
        Task UpdateCarrierInfoAsync(Guid shipmentId, string carrier, string trackingNumber, DateTime updatedAtUtc, CancellationToken ct);
    }
}
