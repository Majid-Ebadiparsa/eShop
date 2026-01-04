namespace SharedService.Contracts.Events;

public record ShipmentCreated(
	Guid ShipmentId,
	Guid OrderId,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;


