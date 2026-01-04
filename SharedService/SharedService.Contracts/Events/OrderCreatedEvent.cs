namespace SharedService.Contracts.Events;

public record OrderCreatedEvent(
	Guid OrderId,
	Guid CustomerId,
	string Street,
	string City,
	string PostalCode,
	List<OrderItem> Items,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
