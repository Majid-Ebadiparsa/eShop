using InvoiceService.Domain.SeedWork;

namespace InvoiceService.Domain.Entities
{
	public class Invoice : BaseEntity, IAggregateRoot
	{
		public string Description { get; private set; } = string.Empty;
		public DateTime DueDate { get; private set; }
		public string Supplier { get; private set; } = string.Empty;


		private readonly List<InvoiceLine> _lines = new();
		public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();


		private Invoice() { }


		public Invoice(string description, DateTime dueDate, string supplier, IEnumerable<InvoiceLine> lines)
		{
			if (string.IsNullOrWhiteSpace(description))
				throw new ArgumentException("Description is required", nameof(description));
			if (string.IsNullOrWhiteSpace(supplier))
				throw new ArgumentException("Supplier is required", nameof(supplier));
			if (lines is null || !lines.Any())
				throw new ArgumentException("At least one line is required", nameof(lines));


			Id = Guid.NewGuid();
			Description = description.Trim();
			Supplier = supplier.Trim();
			DueDate = dueDate;
			_lines.AddRange(lines);
		}
	}
}
