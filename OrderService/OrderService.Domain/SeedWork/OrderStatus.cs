namespace OrderService.Domain.SeedWork
{
	public enum OrderStatus
	{
		Pending = 0,
		InventoryReserved = 1,
		InventoryReservationFailed = 2,
		PaymentAuthorized = 3,
		PaymentCaptured = 4,
		PaymentFailed = 5,
		ShipmentCreated = 6,
		ShipmentDispatched = 7,
		Delivered = 8,
		Cancelled = 9
	}
}

