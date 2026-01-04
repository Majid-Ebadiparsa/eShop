namespace SharedService.Contracts.Events.Invoice;

public record InvoiceSubmitted(
	Guid InvoiceId,
	string Description,
	DateTime DueDate,
	string Supplier,
	IReadOnlyList<InvoiceLineItem> Lines,
	Guid MessageId,
	Guid CorrelationId,
	Guid? CausationId,
	DateTime OccurredAtUtc) : IIntegrationEvent;

