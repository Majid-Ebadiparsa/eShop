using SharedService.Contracts.Events.Invoice;
using System;
using System.Collections.Generic;

namespace InvoiceSubscriber.ConsumerTests.Builders
{
	public class InvoiceSubmittedBuilder
	{
		private Guid _invoiceId = Guid.NewGuid();
		private string _description = "Office supplies";
		private string _supplier = "ACME GmbH";
		private DateTime _dueDate = DateTime.UtcNow.AddDays(7);
		private List<InvoiceLineItem>? _lines = new()
		{
			new InvoiceLineItem("Paper A4", 5.99m, 10),
			new InvoiceLineItem("Pen", 1.99m, 5)
		};
		private Guid _messageId = Guid.NewGuid();
		private Guid _correlationId = Guid.NewGuid();
		private Guid? _causationId = null;
		private DateTime _occurredAtUtc = DateTime.UtcNow;

		public InvoiceSubmittedBuilder WithInvoiceId(Guid id) { _invoiceId = id; return this; }
		public InvoiceSubmittedBuilder WithDescription(string d) { _description = d; return this; }
		public InvoiceSubmittedBuilder WithSupplier(string s) { _supplier = s; return this; }
		public InvoiceSubmittedBuilder WithDueDate(DateTime dt) { _dueDate = dt; return this; }
		public InvoiceSubmittedBuilder WithLines(List<InvoiceLineItem>? lines) { _lines = lines; return this; }
		public InvoiceSubmittedBuilder WithMessageId(Guid id) { _messageId = id; return this; }
		public InvoiceSubmittedBuilder WithCorrelationId(Guid id) { _correlationId = id; return this; }

		public InvoiceSubmitted Build() => new InvoiceSubmitted(
			InvoiceId: _invoiceId,
			Description: _description,
			DueDate: _dueDate,
			Supplier: _supplier,
			Lines: _lines ?? new List<InvoiceLineItem>(),
			MessageId: _messageId,
			CorrelationId: _correlationId,
			CausationId: _causationId,
			OccurredAtUtc: _occurredAtUtc
		);
	}
}
