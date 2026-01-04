namespace SharedService.Contracts.Events.Inventory;

public record InventoryReservationFailed(
	Guid OrderId,
	string Reason,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;

