namespace SharedService.Contracts.Events;

public record ShipmentDelivered(
	Guid ShipmentId,
	Guid OrderId,
	string Carrier,
	string TrackingNumber,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;


