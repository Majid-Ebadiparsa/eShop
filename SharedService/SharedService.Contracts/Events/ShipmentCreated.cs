namespace SharedService.Contracts.Events;

public record ShipmentCreated(
	Guid ShipmentId,
	Guid OrderId,
	string Street,
	string City,
	string Zip,
	string Country,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;


