namespace SharedService.Contracts.Events.Payment;

public record PaymentAuthorized(
	Guid OrderId,
	Guid PaymentId,
	string AuthCode,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
