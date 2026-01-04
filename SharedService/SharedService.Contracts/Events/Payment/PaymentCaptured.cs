namespace SharedService.Contracts.Events.Payment;

public record PaymentCaptured(
	Guid OrderId,
	Guid PaymentId,
	string CaptureId,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
