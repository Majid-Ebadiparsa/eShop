namespace SharedService.Contracts.Events;

public record ServiceHealthChanged(
	string ServiceName,
	bool IsHealthy,
	long ResponseTimeMs,
	DateTime CheckedAt,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;
