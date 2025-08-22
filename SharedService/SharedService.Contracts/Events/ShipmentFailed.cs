namespace SharedService.Contracts.Events
{
	public record ShipmentFailed(Guid ShipmentId, Guid OrderId, string Reason, DateTime OccurredAtUtc);
}


