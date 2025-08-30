using Shared.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSubscriber.ConsumerTests.Fakes
{
	public static class EventFactory
	{
		public static InvoiceSubmitted CreateInvoice(
				Guid? id = null,
				string description = "Office supplies",
				string supplier = "ACME GmbH",
				DateTime? dueDate = null,
				IEnumerable<InvoiceLineItem> lines = null)
		{
			return new InvoiceSubmitted
			{
				InvoiceId = id ?? Guid.NewGuid(),
				Description = description,
				Supplier = supplier,
				DueDate = dueDate ?? DateTime.UtcNow.AddDays(7),
				Lines = lines != null ? new List<InvoiceLineItem>(lines) : new List<InvoiceLineItem>
								{
										new InvoiceLineItem { Description = "Paper A4", Price = 5.99, Quantity = 10 },
										new InvoiceLineItem { Description = "Pen",     Price = 1.99, Quantity = 5  }
								}
			};
		}

		public static InvoiceLineItem Line(string desc, double price, int qty)
				=> new InvoiceLineItem { Description = desc, Price = price, Quantity = qty };
	}
}
