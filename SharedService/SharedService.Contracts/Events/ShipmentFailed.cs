namespace SharedService.Contracts.Events;

public record ShipmentFailed(
	Guid ShipmentId,
	Guid OrderId,
	string Reason,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;


