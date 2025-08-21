namespace DeliveryService.Domain.SeedWork
{
	public enum ShipmentStatus
	{
		Created = 0,
		LabelBooked = 1,
		Dispatched = 2,
		InTransit = 3,
		Delivered = 4,
		BookingFailed = 5,
		DispatchFailed = 6
	}
}
