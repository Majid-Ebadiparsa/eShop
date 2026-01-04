namespace SharedService.Contracts.Events.Payment;

public record PaymentRefunded(
	Guid OrderId,
	Guid PaymentId,
	string RefundId,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
