namespace SharedService.Contracts.Events;

public record ShipmentBooked(
	Guid ShipmentId,
	Guid OrderId,
	string Carrier,
	string TrackingNumber,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;


