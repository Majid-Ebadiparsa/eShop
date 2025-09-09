using Shared.Contracts.Events;
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
						new InvoiceLineItem { Description = "Paper A4", Price = 5.99, Quantity = 10 },
						new InvoiceLineItem { Description = "Pen", Price = 1.99, Quantity = 5 }
				};

		public InvoiceSubmittedBuilder WithInvoiceId(Guid id) { _invoiceId = id; return this; }
		public InvoiceSubmittedBuilder WithDescription(string d) { _description = d; return this; }
		public InvoiceSubmittedBuilder WithSupplier(string s) { _supplier = s; return this; }
		public InvoiceSubmittedBuilder WithDueDate(DateTime dt) { _dueDate = dt; return this; }
		public InvoiceSubmittedBuilder WithLines(List<InvoiceLineItem>? lines) { _lines = lines; return this; }

		public InvoiceSubmitted Build() => new InvoiceSubmitted
		{
			InvoiceId = _invoiceId,
			Description = _description,
			Supplier = _supplier,
			DueDate = _dueDate,
			Lines = _lines
		};
	}
}
