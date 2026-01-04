using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeliveryService.Application.Abstractions
{
    public interface IShipmentProjectionWriter
    {
        Task UpsertShipmentAsync(Guid shipmentId, Guid orderId, DateTime occurredAtUtc, CancellationToken ct);
        Task UpdateStatusAsync(Guid shipmentId, string status, DateTime occurredAtUtc, CancellationToken ct);
    }
}
