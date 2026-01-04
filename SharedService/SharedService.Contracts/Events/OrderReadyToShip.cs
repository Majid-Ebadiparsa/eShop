namespace SharedService.Contracts.Events;

public record OrderReadyToShip(
	Guid OrderId,
	Guid CustomerId,
	ShippingAddress Address,
	IReadOnlyList<OrderItem> Items,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;


