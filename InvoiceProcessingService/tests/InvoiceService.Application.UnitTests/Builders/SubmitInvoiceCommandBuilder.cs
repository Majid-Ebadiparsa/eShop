using InvoiceService.Application.Invoices.Commands;

namespace InvoiceService.Application.UnitTests.Builders
{
	public sealed class SubmitInvoiceCommandBuilder
	{
		private string _description = "Office supplies";
		private DateTime _dueDate = DateTime.UtcNow.AddDays(7);
		private string _supplier = "ACME GmbH";
		private readonly List<SubmitInvoiceLine> _lines = new()
		{
				new SubmitInvoiceLine("Paper A4", 5.99, 10),
				new SubmitInvoiceLine("Pen", 1.99, 5)
		};

		public SubmitInvoiceCommandBuilder WithDescription(string value) { _description = value; return this; }
		public SubmitInvoiceCommandBuilder WithSupplier(string value) { _supplier = value; return this; }
		public SubmitInvoiceCommandBuilder WithDueDate(DateTime value) { _dueDate = value; return this; }
		public SubmitInvoiceCommandBuilder WithLines(params SubmitInvoiceLine[] lines)
		{
			_lines.Clear();
			_lines.AddRange(lines);
			return this;
		}

		public SubmitInvoiceCommand Build()
				=> new(_description, _dueDate, _supplier, _lines.ToList());
	}

}
