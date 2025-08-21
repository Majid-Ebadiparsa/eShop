namespace SharedService.Contracts.Events
{
	public record ShipmentDelivered(Guid ShipmentId, Guid OrderId, string Carrier, string TrackingNumber, DateTime OccurredAtUtc);
}


