using DeliveryService.Domain.SeedWork;

namespace DeliveryService.Domain.AggregatesModel
{
	public class Shipment
	{
		public Guid Id { get; private set; }
		public Guid OrderId { get; private set; }
		public Address Address { get; private set; }
		public IReadOnlyList<ShipmentItem> Items => _items.AsReadOnly();
		private readonly List<ShipmentItem> _items = new();
		public ShipmentStatus Status { get; private set; }
		public string? Carrier { get; private set; }
		public string? Reason { get; private set; }
		public string? TrackingNumber { get; private set; }
		public DateTime CreatedAtUtc { get; private set; }
		public DateTime UpdatedAtUtc { get; private set; }
		public int Version { get; private set; }

		private Shipment() { } // EF

		public static Shipment Create(Guid orderId, Address address, IEnumerable<ShipmentItem> items)
		{
			if (orderId == Guid.Empty) throw new ArgumentException("OrderId is required.");
			var s = new Shipment
			{
				Id = Guid.NewGuid(),
				OrderId = orderId,
				Address = address,
				Status = ShipmentStatus.Created,
				CreatedAtUtc = DateTime.UtcNow,
				UpdatedAtUtc = DateTime.UtcNow
			};
			s._items.AddRange(items);
			return s;
		}

		public void MarkBooked(string carrier, string trackingNumber)
		{
			if (Status != ShipmentStatus.Created) throw new InvalidOperationException("Invalid state");
			Carrier = carrier;
			TrackingNumber = trackingNumber;
			Status = ShipmentStatus.LabelBooked;
			UpdatedAtUtc = DateTime.UtcNow;
		}

		public void MarkDispatched()
		{
			if (Status != ShipmentStatus.LabelBooked) throw new InvalidOperationException("Invalid state");
			Status = ShipmentStatus.Dispatched;
			UpdatedAtUtc = DateTime.UtcNow;
		}

		public void MarkDelivered()
		{
			if (Status is not (ShipmentStatus.Dispatched or ShipmentStatus.InTransit))
				throw new InvalidOperationException("Invalid state");
			Status = ShipmentStatus.Delivered;
			UpdatedAtUtc = DateTime.UtcNow;
		}

		public void MarkBookingFailed(string reason)
		{
			if (Status != ShipmentStatus.Created) throw new InvalidOperationException("Invalid state");
			Status = ShipmentStatus.BookingFailed;
			UpdatedAtUtc = DateTime.UtcNow;
			Reason = reason;
		}
	}
}
