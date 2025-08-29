using System;
using System.Collections.Generic;

namespace Shared.Contracts.Events
{
	public sealed class InvoiceSubmittedEvent
	{
		public Guid InvoiceId { get; set; }
		public string Description { get; set; } = default!;
		public DateTime DueDate { get; set; }
		public string Supplier { get; set; } = default!;
		public IReadOnlyList<InvoiceLineItem> Lines { get; set; } = new List<InvoiceLineItem>();

		public InvoiceSubmittedEvent() { }
	};
}