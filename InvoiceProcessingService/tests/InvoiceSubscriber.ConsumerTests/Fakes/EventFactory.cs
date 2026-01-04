using SharedService.Contracts.Events.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceSubscriber.ConsumerTests.Fakes
{
	public static class EventFactory
	{
		public static InvoiceSubmitted CreateInvoice(
			Guid? id = null,
			string description = "Office supplies",
			string supplier = "ACME GmbH",
			DateTime? dueDate = null,
			IEnumerable<InvoiceLineItem>? lines = null)
		{
			var invoiceLines = lines != null 
				? lines.Where(l => l != null).ToList()
				: new List<InvoiceLineItem>
				{
					new InvoiceLineItem("Paper A4", 5.99m, 10),
					new InvoiceLineItem("Pen", 1.99m, 5)
				};

			return new InvoiceSubmitted(
				InvoiceId: id ?? Guid.NewGuid(),
				Description: description,
				DueDate: dueDate ?? DateTime.UtcNow.AddDays(7),
				Supplier: supplier,
				Lines: invoiceLines,
				MessageId: Guid.NewGuid(),
				CorrelationId: Guid.NewGuid(),
				CausationId: null,
				OccurredAtUtc: DateTime.UtcNow
			);
		}

		public static InvoiceLineItem Line(string desc, decimal price, int qty)
			=> new InvoiceLineItem(desc, price, qty);
	}
}
