namespace SharedService.Contracts.Events
{
	public record ShipmentCreated(Guid ShipmentId, Guid OrderId, DateTime OccurredAtUtc);
}


