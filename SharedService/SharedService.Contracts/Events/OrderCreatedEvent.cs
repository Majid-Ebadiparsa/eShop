namespace SharedService.Contracts.Events;

public record OrderCreatedEvent(
	Guid OrderId,
	List<OrderItem> Items,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
