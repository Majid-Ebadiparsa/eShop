using System;

namespace DeliveryService.Infrastructure.Projections
{
    public class ShipmentView
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string Status { get; set; } = "CREATED";
    }
}
