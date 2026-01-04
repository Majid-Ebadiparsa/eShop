using InvoiceService.Application.Abstractions;
using InvoiceService.Domain.Entities;
using MediatR;
using SharedService.Contracts.Events.Invoice;

namespace InvoiceService.Application.Invoices.Commands
{
	public class SubmitInvoiceCommandHandler : IRequestHandler<SubmitInvoiceCommand, Guid>
	{
		private readonly IInvoiceRepository _invoiceRepository;
		private readonly IEventPublisher _eventPublisher;

		public SubmitInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IEventPublisher eventPublisher)
		{
			_invoiceRepository = invoiceRepository;
			_eventPublisher = eventPublisher;
		}


		public async Task<Guid> Handle(SubmitInvoiceCommand request, CancellationToken ct)
		{
			var lines = request.Lines.Select(l => new InvoiceLine(l.Description, l.Price, l.Quantity)).ToList();
			var invoice = new Invoice(request.Description, request.DueDate, request.Supplier, lines);

			await _invoiceRepository.AddAsync(invoice, ct); // Transactional with Outbox (configured in Infrastructure)

			// Publish domain integration event (goes to Outbox first, then RabbitMQ)
			var eventLines = invoice.Lines
				.Select(x => new InvoiceLineItem(x.Description, (decimal)x.Price, x.Quantity))
				.ToList();

			await _eventPublisher.PublishInvoiceSubmittedAsync(
				invoice.Id,
				invoice.Description,
				invoice.DueDate,
				invoice.Supplier,
				eventLines,
				invoice.Id, // Use InvoiceId as CorrelationId
				ct);

			return invoice.Id;
		}
	}
}
