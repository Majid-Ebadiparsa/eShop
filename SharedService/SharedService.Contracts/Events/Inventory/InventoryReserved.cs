namespace SharedService.Contracts.Events.Inventory;

public record InventoryReserved(
	Guid OrderId,
	decimal TotalAmount,
	string Currency,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
