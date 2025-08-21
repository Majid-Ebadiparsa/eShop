namespace SharedService.Contracts.Events
{
	public record ShipmentDispatched(Guid ShipmentId, Guid OrderId, string Carrier, string TrackingNumber, DateTime OccurredAtUtc);
}


