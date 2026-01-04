namespace SharedService.Contracts.Events.Inventory;

public record InventoryReleaseRequested(
	Guid OrderId,
	List<OrderItem> Items,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;

