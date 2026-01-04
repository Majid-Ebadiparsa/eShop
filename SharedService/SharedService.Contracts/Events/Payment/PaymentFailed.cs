namespace SharedService.Contracts.Events.Payment;

public record PaymentFailed(
	Guid OrderId,
	string Reason,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
