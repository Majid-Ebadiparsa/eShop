namespace SharedService.Contracts.Events
{
	public record ShipmentBooked(Guid ShipmentId, Guid OrderId, string Carrier, string TrackingNumber, DateTime OccurredAtUtc);
}


