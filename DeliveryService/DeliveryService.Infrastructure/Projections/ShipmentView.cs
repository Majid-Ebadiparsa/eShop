using System;

namespace DeliveryService.Infrastructure.Projections
{
    public class ShipmentView
    {
        public Guid ShipmentId { get; set; }
        public Guid OrderId { get; set; }
        public string Status { get; set; } = "CREATED";
        public string? Carrier { get; set; }
        public string? TrackingNumber { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
