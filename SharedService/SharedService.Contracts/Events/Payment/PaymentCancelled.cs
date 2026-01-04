namespace SharedService.Contracts.Events.Payment;

public record PaymentCancelled(
	Guid OrderId,
	Guid PaymentId,
	string Reason,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
