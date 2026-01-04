namespace SharedService.Contracts;

/// <summary>
/// Base interface for all integration events exchanged between services.
/// Provides standard metadata for distributed tracing, idempotency, and event sourcing.
/// </summary>
public interface IIntegrationEvent
{
	/// <summary>
	/// Unique identifier for this event message.
	/// Used for idempotency checks to prevent duplicate processing.
	/// </summary>
	Guid MessageId { get; init; }

	/// <summary>
	/// Correlation identifier for tracking related events across service boundaries.
	/// All events in a logical flow (e.g., order creation -> payment -> delivery) share the same CorrelationId.
	/// </summary>
	Guid CorrelationId { get; init; }

	/// <summary>
	/// Identifier of the message/event that caused this event to be published.
	/// Forms a causal chain for debugging and tracing.
	/// </summary>
	Guid? CausationId { get; init; }

	/// <summary>
	/// UTC timestamp when the event occurred in the source system.
	/// </summary>
	DateTime OccurredAtUtc { get; init; }
}

